using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Services;
using AvePoint.Wrapper.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IEmailTemplateService = AvePoint.RA.Contract.RMWeb.IEmailTemplateService;

namespace AvePoint.RA.Api.Web.Controllers.ResourceApi
{
    [Route("api/CpApi/[action]")]
    [ApiController]
    public class CPResourceApiController : RAWebApiBase
    {
        private readonly IRALogger _logger = new RALogger(typeof(CPResourceApiController));
        private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService<IManualProcessManagementService>();
        private IEmailTemplateService EmailTemplateService => PlatformWindsorManager.GetService<IEmailTemplateService>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

        #region manual process
        [HttpPost]
        public Task<RAReturnMessage> SaveManualProcess([FromBody] WorkflowDefinitionDto dto)
        {
            return ManualProcessManagementService.ApplyManualProcessAsync(dto);
        }

        [HttpPost]
        public Task<RAReturnMessage> DeleteManualProcess([FromBody] Guid id)
        {
            if (id == Guid.Empty)
            {
                return Task.FromResult(new RAReturnMessage
                {
                    MessageType = RAMessageType.Successful,
                });
            }

            var workflow = ManualProcessManagementService.LoadProcess(id);
            if (workflow != null)
            {
                return ManualProcessManagementService.DeleteProcessAsync(id);
            }

            workflow = ManualProcessManagementService.GetWorkflow(id);
            if (workflow == null)
            {
                return Task.FromResult(new RAReturnMessage
                {
                    MessageType = RAMessageType.Successful,
                });
            }

            return ManualProcessManagementService.DeleteProcessAsync(workflow.Id);
        }

        [HttpPost]
        public async Task<bool> SyncUsers([FromBody] List<AccountDto> accounts)
        {
            try
            {
                _logger.Info("sync user to other DC");
                await UserService.BatchAddAccountsAsync(accounts);
                return true;
            }
            catch (Exception e)
            {
                _logger.Error("sync user to other DC failed", e);
                return false;
            }
        }
        [HttpPost]
        public async Task<bool> SyncCommonDataUsersInfo([FromBody] SyncCommonDataUserInfo commonUser)
        {
            try
            {
                _logger.Info("sync user info to main DC");
                await UserService.SyncCommonUsersInfoToMainDCAsync(commonUser);
                return true;
            }
            catch (Exception e)
            {
                _logger.Error("sync user info to main DC failed", e);
                return false;
            }
        }

        #endregion

        #region email template
        [HttpPost]
        public string CreateEmailTemplate([FromBody] EmailTemplateDto eamil)
        {
            return EmailTemplateService.CreateEmailTemplate(eamil);
        }

        [HttpPost]
        public string DeleteEmailTemplate([FromBody] Guid uniqueId)
        {
            return EmailTemplateService.DeleteEmailTemplate(uniqueId);
        }

        [HttpPost]
        public string EditEamilTemplate([FromBody] EmailTemplateDto eamil)
        {
            return EmailTemplateService.UpdateEmailTemplate(eamil);
        }

        [HttpPost]
        public void UploadImage([FromBody] EmailImageDto email)
        {
            if (!ValidateUploadImage(email))
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            EmailTemplateService.UploadImageToOtherDC(email);
        }
        private const int MaxImageSize = 10 * 1024 * 1024; // 10 MB

        private bool ValidateUploadImage(EmailImageDto email)
        {
            if (email == null || string.IsNullOrWhiteSpace(email.Base64))
            {
                return false;
            }

            byte[] imageBytes;
            try
            {
                var base64 = email.Base64.Trim();

                // Support data:image/png;base64,...
                var commaIndex = base64.IndexOf(',');
                if (commaIndex >= 0)
                {
                    base64 = base64[(commaIndex + 1)..];
                }

                imageBytes = Convert.FromBase64String(base64);
            }
            catch
            {
                return false;
            }

            // Check max size (10 MB)
            if (imageBytes.Length > MaxImageSize)
            {
                return false;
            }

            return IsSupportedImage(imageBytes);
        }

        private static bool IsSupportedImage(byte[] bytes)
        {
            if (bytes == null)
            {
                return false;
            }

            return IsJpeg(bytes)
                || IsPng(bytes)
                || IsGif(bytes)
                || IsBmp(bytes);
        }

        private static bool IsJpeg(byte[] bytes)
        {
            // JPEG starts with FF D8 and ends with FF D9
            return bytes.Length >= 4 &&
                   bytes[0] == 0xFF &&
                   bytes[1] == 0xD8 &&
                   bytes[^2] == 0xFF &&
                   bytes[^1] == 0xD9;
        }

        private static bool IsPng(byte[] bytes)
        {
            byte[] signature =
            {
                0x89, 0x50, 0x4E, 0x47,
                0x0D, 0x0A, 0x1A, 0x0A
            };

            if (bytes.Length < signature.Length)
            {
                return false;
            }

            for (int i = 0; i < signature.Length; i++)
            {
                if (bytes[i] != signature[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsGif(byte[] bytes)
        {
            if (bytes.Length < 6)
            {
                return false;
            }

            return bytes[0] == 'G'
                && bytes[1] == 'I'
                && bytes[2] == 'F'
                && bytes[3] == '8'
                && (bytes[4] == '7' || bytes[4] == '9')
                && bytes[5] == 'a';
        }

        private static bool IsBmp(byte[] bytes)
        {
            return bytes.Length >= 2 &&
                   bytes[0] == 'B' &&
                   bytes[1] == 'M';
        }
        #endregion
    }
}
