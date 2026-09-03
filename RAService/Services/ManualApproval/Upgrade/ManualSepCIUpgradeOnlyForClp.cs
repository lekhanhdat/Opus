/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ManualApproval.Upgrade
{
    public class ManualSepCIUpgradeOnlyForClp
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(ManualSepCIUpgradeOnlyForClp));

        private readonly List<string> ApprovedIntanceIds = new List<string>
        {
            "e3f0fca3-bf13-4f94-819e-38873c2e14a9",
            "c44e0b32-6246-4e07-8c04-c631c6aece41",
            "3492beb9-d3d9-4adf-a6d2-03a4cb1e50fe",
            "92e95cf1-df32-40a1-aaed-33cfe186c557",
            "c7ab82a8-64ab-41e6-8f4e-b93704ab47c3",
            "58e14fe9-8d3d-4777-ada9-6a9252c2f3a7",
            "78ac1536-1d35-432d-be79-37058e2fd985",
            "da700954-37e3-4f56-a0dc-9124efc8bd24",
            "fd245cfe-0440-45e5-8e25-03baa31da426",
            "c3859226-7c26-4421-b90b-8aa71e76fb28",
            "e541657e-d1b7-41ec-acbb-ad873fbc5db3",
            "1ec50a24-e4f9-4705-a6b3-ccfa13535b63",
            "9879b21d-b93d-46d4-ad93-b0248ceaed3e",
            "d516e786-d3b2-4b66-94cb-ae2124dfea95",
            "ad529625-65df-4a2b-8648-052fb3665f4e",
            "84cdc873-e100-4d6e-82f8-c878b87510af",
            "32e26e00-0838-4323-9333-689501606f18",
            "1798f8d6-0b94-4c12-a705-b2e79e14b9dd",
            "fce55760-a8ce-4e9c-a3d7-7c887c2ca39f",
            "633b89a4-4dce-4dd8-880a-c6d3dbf6b1cf",
            "2d2fc8cc-42d4-4580-bb13-81011881c8f4",
            "ac24eedd-c598-4bad-ae69-ce9719e9b43d",
            "b90b6f17-d49e-4f2c-baa9-075c6783c74b",
            "5a68f0f4-711f-4b2d-933d-0346132f891b",
            "411ee80d-f47d-4a5a-a7de-4cfa17d15141",
            "bc632f7c-6862-4c7c-b609-c574cf878046",
            "7f19aa24-163f-47d5-b01c-8adaff3dde28",
            "efa0985f-bc00-49e4-b51b-517d78dd26ee",
            "d2560326-ac84-4098-9a61-df555893c034",
            "613e1966-fd6c-4813-adb6-9a2454ae4ec9",
            "e26d6b5a-013a-4a54-bcd1-880731cdbbe4",
            "95426628-4925-4591-a908-ca1e3d85c58e",
            "8ce5db0e-8242-4b45-aa88-b838ca7779fe",
            "e86db4d9-69cf-43e5-8e6c-bef6ddd64748",
            "419b6bf4-19bc-4d0f-8205-3b0a12a7e811",
            "052f3e95-a046-4c2b-993c-4d63475af707",
            "7d1b2427-e1ca-4aae-b3bc-5aee873e18f9",
            "22bba069-89c1-4468-b8be-94425d737eff",
            "81df4265-5b56-4fc9-b280-b83b82bc0afe",
            "d7a45fb1-8d19-44a1-89b7-cf1536564ee8",
            "ecf2e5c9-e809-4163-8f68-9cbcf428d0a9",
            "b2e2026c-b240-4106-86f8-af04cfaa6d19",
            "cb960e91-e9a8-4649-9710-d7bd6ac279fc",
            "ad83cd3e-4f48-4823-b8b9-d199bee54876",
            "ac07bebc-3875-4b05-a70a-7fc988b1e625",
            "a8e092ad-0ff0-4027-9e7d-d72fba9d9c09",
            "d71fada4-a3b0-484c-9e11-3d54507f9093",
            "4b106556-e0c7-4982-a9af-33449c3b37c4",
            "95011283-9f25-4344-bcfe-7ee0cc49c078",
            "09ae82a7-b8a7-41d8-a122-e0c4b1a807d0",
            "6668ae2e-2753-40fc-b59d-941b53e452e9",
            "fe69a777-bf7c-4275-bd11-6001bb8ee2f6",
            "3a270d00-eec7-47ca-9ce2-37a24cb02929",
            "b756a1b2-4d76-4205-8f9b-b63743baf3ba",
            "19d23f8c-340b-4e1c-becb-4246b9643e32",
            "83b7ed4c-31f8-4a6b-89ae-2c3fdf75ae1b",
            "3d8aa109-f85e-4845-929b-5689ce9d7412",
            "eb928b69-75ab-470e-b589-c6a0fd0d5a52",
            "e07bcf19-2c0f-4a76-b795-7a5f9bd20761",
            "075e9561-cce9-4e33-b89a-afb90b9f8b93",
            "6a3f6a79-b8f8-48a4-bf82-fd074d7d182d",
            "d1261b0d-5f86-4ef7-bbac-fbe5c33b17d1",
            "5314b6cf-895d-4eb6-b678-d6a1e240c704",
            "41fcc68f-3374-4411-b27d-8886f81c6ae9",
            "84b54995-b40f-4598-ab82-18a0585b1748",
            "f961a703-b55b-4122-bcca-15da591bd1fe",
            "640b885a-158a-4ef9-a609-6adc0a76bf82",
            "da998df0-6ef9-488a-9ccc-5b28fa64e1c5",
            "b6f1482a-ba1e-40c3-83ee-46facf1ee4c8",
            "d0f754be-4b00-4d22-ab91-9dc5a6b784fa",
            "8229aaed-a9b8-49c1-b24c-d3643e0e9204",
            "f6a4b703-2239-4b5b-bc66-5376ff8af6c6",
            "fdb6e224-8485-428f-883f-aaacb08f3e91",
            "17c60089-71ba-408a-b1e0-cfb5084b26ef",
            "a4c4364b-9665-47b5-b726-3389fc49ed11",
            "65b50090-bb0d-43bf-90a6-cf61d1961166",
            "c264935a-ecba-4153-88c8-97141c414ffc",
            "97f5a7f5-48f8-4438-858b-cc98db6f4254",
            "f7ee7f96-96bb-40ff-b00d-2751184bba1c",
            "dc7e8e8e-c59c-4c66-b12a-71e7afaf5be7",
            "c333a7d0-b38b-4588-91f1-e2e81474beb4",
            "20d0d0f9-2524-4be2-ae30-4b18bbc0ad07",
            "135c2346-1c75-45b3-a0fe-9a82e1aef162",
            "29c7aca1-cd04-4e36-84c2-472607fcce45",
            "e324d3fc-82c3-41d9-ad45-6e25b8459434",
            "30890af8-384b-4085-8e4a-841f5975bad6",
            "eea1c71f-767c-47c8-a571-d693a615a5df",
            "92b07837-a6aa-403e-b246-987e5460a18b",
            "0c10897a-8382-4824-a6ec-925e34cf1b0f",
            "cbd195ae-46ae-4e60-adf7-c4d01c5fe58b",
            "ae5d6685-f336-40dc-8df7-b89904ebc54d",
            "7148f7b7-e189-4bcb-ab7a-7a107d72635c",
            "7b56f09f-570e-4a69-8432-a41752eab982",
            "d2a22b06-5fac-41e6-8b5d-208375639e78",
            "99cfaaca-2097-4e87-abc1-fa549629bbfe",
            "9c51f938-83d8-4193-9cd0-059e14479596",
            "7f1fd252-2793-45c7-b317-08479523ce21",
            "4d5a17aa-5c70-4eaf-94e3-2235bd89ad59",
            "11578474-0a8f-4efb-bdb5-56daf3a72648",
            "b4e0006f-2edf-468b-827e-a967644ddd03",
            "8eaf2942-6b3d-4b3e-b4cc-1b202a0d4042",
            "54006150-4386-4774-94b8-028bf72c5437",
            "d42439f7-a7a1-4473-a895-038e8b8ac4b6",
            "ec7668cb-ea2d-40f3-910c-ce10d24a6a73",
            "03f495ce-a7bc-439d-9888-dbdec6e4ea43",
            "8bd4257c-508a-48da-9caf-7a0407585328",
            "f7538e1d-5f1b-4d66-ac7a-56a35f7ded53",
            "c2e6a6b6-f5dc-4787-b4be-6156fe1a0213",
            "e3411ab8-1913-4c77-aaa9-becbaddcf238",
            "057c62d6-573b-4c4d-915e-a574ba540a6a",
            "57b8f072-0f29-4f4c-8527-0fed4c28374f",
            "c2bfbd4c-963c-4ad0-8f89-6a3f809385d7",
            "4edee5f6-9e58-4276-b679-2d8881e2a07b",
            "57fa98cc-666d-4e9e-81c5-4aa0e15871ce",
            "d4a3e07b-413f-4a17-a821-a467ba5cef32",
            "9bc41e81-3765-4592-9815-d00264eedfa7",
            "cb21a58d-ada1-48ea-b97b-068f45348702",
            "03357de7-3f39-4c6f-b75b-9c77f94dc2ea",
            "8856df25-8f12-48e9-934c-e309f4fe26bf",
            "4e27c193-d941-4c54-99cf-c0ebe3b0be7f",
            "29fc4293-f355-44df-8a74-39a6b23137b7",
            "0a6f06f6-a5d1-432c-9cd7-01a8d8575234",
            "cc7c0d20-ca1a-4f88-a5f4-39ba729421d3",
            "58b26975-36e4-4539-bbd2-8aeef9428e1a",
            "e48be682-6a4e-442f-9a78-d493087aff84",
            "3cf1736a-d08d-4edf-9787-2070a9c8aacf",
            "a2e728f0-15be-40f5-9121-a94f36d4cb6b",
            "26b4f377-195b-43af-a3e9-b2f956b0d71b",
            "ad98bcc5-50a0-4964-b8b8-7f885c7de21b",
            "7a3c6f73-1316-4fee-9852-a20db2b49952",
            "110af6d2-a2aa-4018-b758-d593dc9534e3",
            "c35fc88e-3ea2-4edb-867a-04aa317e5722",
            "4f08b72d-3c48-4b2a-806a-032d88c9312a",
            "79935389-4f4c-4d69-b70c-228f4c241dde",
            "fe906d4a-0177-4560-b13e-df0b8520d086",
            "e7250fcc-f1f7-493c-a623-0d2de3eafb0a",
            "aaa43d15-58fb-4aa9-9588-5316433ee779",
            "5a141358-3710-4910-adc4-f2c822be8ff5",
            "9872341f-950f-43a8-8f92-06defb773442",
            "9da67250-5d17-423f-9c23-cd93949a7db7",
            "e3f0fca3-bf13-4f94-819e-38873c2e14a9",
            "c44e0b32-6246-4e07-8c04-c631c6aece41",
            "3492beb9-d3d9-4adf-a6d2-03a4cb1e50fe",
            "92e95cf1-df32-40a1-aaed-33cfe186c557",
            "c7ab82a8-64ab-41e6-8f4e-b93704ab47c3",
            "58e14fe9-8d3d-4777-ada9-6a9252c2f3a7",
            "78ac1536-1d35-432d-be79-37058e2fd985",
            "da700954-37e3-4f56-a0dc-9124efc8bd24",
            "fd245cfe-0440-45e5-8e25-03baa31da426",
            "c3859226-7c26-4421-b90b-8aa71e76fb28",
            "e541657e-d1b7-41ec-acbb-ad873fbc5db3",
            "1ec50a24-e4f9-4705-a6b3-ccfa13535b63",
            "9879b21d-b93d-46d4-ad93-b0248ceaed3e",
            "d516e786-d3b2-4b66-94cb-ae2124dfea95",
            "ad529625-65df-4a2b-8648-052fb3665f4e",
            "84cdc873-e100-4d6e-82f8-c878b87510af",
            "32e26e00-0838-4323-9333-689501606f18",
            "1798f8d6-0b94-4c12-a705-b2e79e14b9dd",
            "fce55760-a8ce-4e9c-a3d7-7c887c2ca39f",
            "633b89a4-4dce-4dd8-880a-c6d3dbf6b1cf",
            "2d2fc8cc-42d4-4580-bb13-81011881c8f4",
            "ac24eedd-c598-4bad-ae69-ce9719e9b43d",
            "b90b6f17-d49e-4f2c-baa9-075c6783c74b",
            "0c46e11a-91aa-43a3-9604-10696d2e716d",
            "a0e5f69e-6898-4965-92fe-f9032f0cee8b",
            "e2d44b5a-1762-4ef4-bf2f-6b910c704386",
            "76c2ff01-a0d8-4338-bbb6-8dee23acc7e8",
            "8c8fa103-6023-427b-8ff7-64f330e58f07",
            "dd754dae-8cf0-40ba-9d60-443314523b9d",
            "e88055de-8321-475c-9510-ae72bb10aa2f",
            "9d3d4ab8-76f8-431d-968f-04f003cd8134",
            "5af8073d-8bae-4e23-b276-07bf91c4aa98",
            "fd034e18-86ba-4331-814b-e97db591b4c3",
            "89cd459b-c768-4d3f-b633-50ec5675e167",
            "99a5670a-d5f9-4f97-ab36-0efa01694140",
            "f2a16f64-9ac5-4646-9aed-697a6ca7fd6d",
            "a5fad1aa-726e-47e5-8c82-584e9b97bb35",
            "a585c260-f779-46ff-b82e-5cc805ac1b99",
            "f1ba4e79-01cc-45e9-bf12-eafb0b6780d7",
            "6600d24f-a585-453c-94eb-803544c382fc",
            "7ebbdcfb-786d-4b52-9c7d-cc53fcb7d7f8",
            "20b9bc74-528c-4f74-a58f-2908665c01ff",
            "98604f3a-d965-4316-b37b-1bdd1b151e72",
            "88dfed29-3f65-48fc-8724-c774d5dbfaf7",
            "09814fd9-545e-40fd-86eb-dcfee74eaffe",
            "9ce0ae6e-325a-4613-af24-ae980a786bd3",
            "3a34d558-596b-4bbf-936f-bf2bd5180325",
            "6367a32d-2226-49b1-9d85-4a9e29319b8f",
            "6bfd1cb5-9c63-444a-a495-1c119015fe0e",
            "0a0d1349-ad4c-4347-80ad-65abbfa78f6a",
            "5330341d-e6a7-4eb1-8855-e6f9da40fa80",
            "4290a115-b87c-4a3c-80e6-913b57d4c89e",
            "fff2bbd5-d2a4-4325-9a23-07fb4f95b447",
            "1cf3d62b-2aee-44d0-b49c-19ac73192a51",
            "890445b9-6444-4acb-a92a-6fa47938a10b",
            "28fa1e58-2c54-460a-9edc-6206ae276b9a",
            "6952d47c-4347-4c37-b9c3-0eb7f09358f8",
            "6471f821-39c7-4c7b-86e6-ce5aa86cb79b",
            "107accd6-b673-4d01-a8c9-23c016ad9264",
            "1d8a0167-6d00-4e59-83b2-13c467a8eb73",
            "419c2d5b-5f17-445a-9166-4c812d4234ae",
            "d6d07bc1-be3e-4016-9aef-657a0746e6df",
            "e5452314-4d3f-4b13-abb9-7ede85ecf2ef",
            "2c3efce2-4d06-4647-b1c6-1912805554c5",
            "9c96b856-aec2-49a5-bf62-fe903946e87d",
            "c94565c6-3991-4371-939e-f4d0329b2b74",
            "c6bc30f6-cfbd-40d7-8b2a-e0e1094e51d3",
            "ce466a87-b920-44a4-8535-3cf5a30fb245",
            "61d8326e-e634-4da2-957f-c95e4018ff1f",
            "46efa113-cc13-4fc9-ada6-a451002f3913",
            "b856890c-fcdb-4321-bc1e-b24aa177c57b",
            "27d020cb-2b69-40d3-8d4c-a1b7cadb40f2",
            "17756a4c-71f9-4b90-b6de-f999b052f5af",
            "d71e988f-748b-4961-862d-84cc3d52c5e1",
            "c7e638e1-dea9-4a2f-b2b4-741fe1fde782",
            "a5894536-788f-493f-91f9-f696051b2298",
            "7f1814ec-e06c-4b94-955b-f55bc961f3e2",
            "cbb14741-cef8-4799-9c83-b419336e6b72",
            "29664f0a-11cd-4759-885f-a3acd142ea78",
            "c86b5005-e103-4e02-b20e-9039cba00029",
            "eef921c5-c4b8-415a-99a5-54a8ba813a08",
            "d3803d5e-98f5-421b-b37b-c7fb2c9484ad",
            "838b06e4-b392-4a4d-98b8-a1656928b4ae",
            "4e60d3da-7b0f-4344-a23f-6bcd87d7b3a4",
            "0af1aeb9-f200-4fea-8ea5-073b9011820b",
            "a054651f-08f6-40e3-9129-9a6bef17dc56",
            "daf66b30-d6a7-4c6e-90d2-b7c27d8d820a",
            "4dcaf683-f1c4-48c5-b95c-465e2d43214c",
            "afdbefa0-1596-4785-817f-6fbb2540f419",
            "3fc7193c-c50d-4055-a795-190552cc66ae",
            "afa4167f-2591-47d3-b4fc-38cdf4f124db",
            "7e264573-e684-4db5-a60c-0bfeef8034ec",
            "eb33229b-4af0-4f52-be1e-82ddc580e535",
            "462b2bfc-aeb0-43ef-a55f-b6ebbf026bda",
            "2fd832bd-e371-4efb-8dcb-2bbf4cd9ec16",
            "2109e14d-8355-457e-9106-1f6e5a00eb3d",
            "d68f86f5-10c0-4a7c-9403-1c9ea320b504",
            "591c0642-0c9f-47e7-9b10-4f9e29561b7f",
            "162def88-cae5-4754-a679-fda9609f824c",
            "d2d8531c-4c94-4861-8ed0-2b0b21a40b29",
            "4bd5b3cd-cdae-429e-bca5-58538840be10",
            "8e018207-7c7b-477c-b4da-99093008e7a9",
            "a896a924-8a74-4f9a-8283-01edb650de29",
            "67a86164-0615-4129-9260-5450a0dcc22c",
            "507721d2-6da5-4e54-98b2-53a1e36c8120",
            "564d9c97-47d5-48fb-bba7-ad9c386e6ce6",
            "cd416f54-2ab5-4552-a7e8-bacdfcffbec2",
            "32e55c83-68a7-4d9c-85de-e2b4c1c16383",
            "336c3188-0369-4494-8912-e5455ffb956b",
            "f318f5fc-97e3-4915-9bec-f3151cafde26",
            "9804d40f-8ff6-4ae0-b7fc-c3631b60778d",
            "739770f3-7943-4f8a-bc1f-a78d78095fe4",
            "426dfa75-4f8b-407a-8e7d-7843511d3bb6",
            "d90c99e8-d4c6-4151-9f22-77b8c5dc2c78",
            "438f5d72-c5ef-4fff-b647-b3b4d0889542",
            "4009491a-9246-4645-9602-04ba1c65f88e",
            "cf7bbe15-bb05-47f2-a6d7-6408e2c807ff",
            "a7b09deb-4a5d-4684-9b26-ee941ca46271",
            "f8263b8c-9921-40d9-ab08-8ace26e5bc97",
            "39112f47-c511-4b8f-8507-e51a8ed77686",
            "5717432f-d0c9-4d24-b3a8-80cf2db7155f",
            "72e7f352-97b0-4a5e-bf5f-764e5a472b65",
            "3b5c651c-a1e0-4c26-9b11-acf1eef7a447",
            "8e421953-5274-43c4-848e-441dd3258b6e",
            "9f8690e8-4966-4a48-a06b-828a74d78315",
            "2d246741-b9c5-407c-a892-c87c543eadd4",
            "7786fa9d-8353-4969-a1da-c6f696f6151c",
            "6f22baf2-1177-451d-aead-aa4050b60578",
            "c3414718-e924-4339-9493-a10ed1ab1062",
            "41261735-42ac-4e58-b264-e322baec786a",
            "5032b39e-2fc9-49ee-aba7-864ac737dbd9",
            "e5dfc5a6-b1bc-4795-a8cc-d48c7410542a",
            "47267773-6c6b-46f1-952b-a37070b9255c",
            "48cccfe7-4a0c-404a-9444-ca89f87d182a",
            "67f44c89-8e5e-4840-8649-eaba751399dd",
            "a48fb4e5-05ee-4316-a40f-0b80f107266d",
            "d29c8758-6ec5-424f-bda2-7555c1962702",
            "98864413-3130-48ae-a883-0c3a11d67740",
            "63b1ee36-3b6d-4183-b3f2-291c8dcb6038",
            "e260bcfb-e2c1-44a5-ab6b-ede666b14747",
            "bc7afdc4-d269-4bfd-a92d-d855b757fc96",
            "8b32f7e2-88fc-482b-bcab-e223c3b99dbe",
            "04c4311b-e56d-4498-9b61-fadb40f2ad7d",
            "68ee2079-e2c1-4612-aecb-9a27d88dc29d",
            "88db33d2-9ed9-49de-b973-ee33ed2ae042",
            "539b6f75-b41c-4fe8-96f8-1c540b0af205",
            "0ba4d4a9-6c1a-4882-ad04-d31babbb6c23",
            "eeee02d8-971d-4818-b791-291acf79a73c",
            "450fe9f7-e5f3-422a-8472-22c05544480f",
            "dced03d1-7c08-43f3-8f55-71167500e8e8",
            "8f217a00-9223-40bc-beb6-86c52b1b6913",
            "b28dee0b-fcc9-4ab6-b762-7fe8ecbb9f16",
            "4108bdc7-e771-4ac5-a4e8-3445286f3f72",
            "0e413b3a-54b4-440c-8999-a70923387c81",
            "8c7ab71b-c2c7-4a9d-9e56-a49cef60889d",
            "df186cb4-384d-4f90-9253-c627dd19d627",
            "6ef4c383-3ff8-452c-8dfa-27711238f7fa",
            "8b769099-2f1e-45e2-ae74-a9bce6f3b498",
            "7ca34ac4-c37d-49bd-abf8-a94744ed1f14",
            "0cdd8e02-f089-4b5d-8df7-b0be56d5cc86",
            "68faf1e3-9963-4d93-9042-21715bda50cc",
            "d8c1f17b-0e84-4d6a-ad12-66c78a02d6cb",
            "d4ef75e8-11e6-4edc-9e59-5388240d0f38",
            "e9782ea3-46a6-4640-a5af-1146458ef945",
            "f1b12ef0-160b-4370-989d-8640f7c1e629",
            "fdcbbbaf-bde7-44fe-8a87-fa04cdb3451f",
            "bf491d8f-58a3-430e-a837-0f70984ff2bd",
            "5e3dd927-6ece-47f9-8316-8da7aaefbb21",
            "054b49f4-0642-49df-87cf-9c1ea734675b",
            "af415b8f-ddaf-430c-9636-83e1acd5cb51",
            "a05b493c-25e5-4a97-9819-59356afb38a3",
            "86b68db2-03b5-49d8-a9a0-ca16e8e92e37",
            "afd42ed4-1938-498e-bb34-f300bd29bc06",
            "47e921f4-ac20-496a-9c4c-9855cb35a201",
            "de17208c-b281-4263-b3a1-40a572eb460a",
            "2622a89d-d9e6-4270-a42b-269e761882c2",
            "48cda4cb-5db0-45c1-b1cc-a9e4f93ae8b1",
            "d2a4ed7e-3697-4725-843d-d1528e724b93",
            "04f355b7-b51c-43e7-bcd9-a406bbd7f5f7",
            "2ce60649-ecea-462c-b1b7-19287cb76a43",
            "3bd6bf9e-7739-4b14-89c4-4212e66a56b1",
            "976710bc-80be-4a38-891c-1ae46784b3b7",
            "49de6697-6bbc-45d6-aab1-a852126e2b04",
            "46ec5f14-c397-458f-9168-cbcb53139335",
            "d07d0c3d-db49-4c07-a8f9-f16150d6b384",
            "6bb9743a-9d55-4bb2-abed-868c7d03708b",
            "8341058c-094d-418c-b79f-dd1986163f47",
            "f1b51d36-467f-4b11-8e3d-dacbb3bda03d",
            "bfaf3a1e-22fb-4bc6-b3a8-a9e7b4f0949c",
            "b113d073-677a-45bf-a633-57544571ccd8",
            "8d334bb6-ba1a-44a8-ba5c-7a84d351e681",
            "d566defc-ed2b-4554-859c-0099a19a1d79",
            "9abb55a6-ec63-4c33-b900-c98c4ac3d6fe",
            "d01ba2bb-4373-442d-866f-ede96003447b",
            "8df4d3bd-3428-4e92-acc7-3416edc6b6ad",
            "f8a4d495-4d16-4d90-93cd-e1917de2f57a",
            "c559265e-9b69-4d62-9ffa-4a0bc65e8693",
            "9822d42f-ab5f-48e1-b722-175e1586611b",
            "bbbcfe87-d86a-4543-9cd6-ee057018ca86",
            "4958111f-4a54-4c04-b528-270d6f5fbcdd",
            "e30188e4-0e75-4dbf-b800-bceecc05cf26",
            "c3e2b319-535e-4ce2-8cc4-23136d38e669",
            "20bac134-fe6f-4078-8ed5-460454a1f5b7",
            "25914ea9-0761-4bac-ad0e-516af03b3431",
            "cef8d595-8a92-4ec5-af65-b5268f434203",
            "12b9e208-5b0a-43d2-9c1d-adb04a60507b",
            "61be17a5-8a21-4a1a-b74c-5377ffc1519a",
            "f79476cd-720b-4804-87a2-09269df2f1ef",
            "d3380e3a-794a-40c7-9d2f-53c09591f350",
            "de1a0e46-d429-4f01-8716-a163d662b90a",
            "4bfbc849-e290-4659-8c7e-c2fb508d8c46",
            "bf9e5b5d-35e4-440e-911b-39b18285a5df",
            "e81306e3-f761-4b53-b0af-aad4aa748b4d",
            "33065255-8da9-4786-a068-920764650467",
            "2602b155-966e-40bd-a867-8ec33f1c8fe4",
            "42674c69-d153-43ce-b844-2009daac4038",
            "ff9a8b00-8556-4277-bcb9-a6f1c77ae3d6",
            "df33f9c4-6d27-4a90-9417-2a3b9c58dc56",
            "8b81caec-23ad-4c62-a3b7-b3a1e2bdab77",
            "0bb837cc-0577-43a6-8440-8312977f1d6f",
            "da06f86a-3a83-4600-a699-e467c4bb95c0",
            "cc027f93-d683-41a8-9cc0-5ed2a28a28d6",
            "854f84c6-7dc8-4ae4-9c15-edbd0d6ffa34",
            "b3e768c5-fdf1-47dc-8746-3a75ff0457ab",
            "27f508ea-dbd2-4fbf-ab13-e54bfec52176",
            "45d22b8a-37a1-447c-938d-dbb36f7e5b2c",
            "74874ed8-d638-4047-9a33-71935698338e",
            "5bc7d39a-c081-4590-9659-df710b4f8446",
            "30a6434f-fd0b-481f-afae-3b1572e3e636",
            "0e5bd8cb-4c3c-4ca6-a7ba-bfcb0c0d0003",
            "d9440e30-dfbd-415e-8c77-c70c13afac79",
            "7ca27b9e-3bc1-4e00-a205-189eac25e1df",
            "82bf11c1-65e6-4b0b-add7-d619ef367eac",
            "dc0a73e3-f2cb-4d5a-a7a8-ccac7d1837d0",
            "9ac9ffbc-29f7-4c99-a3d5-4a9a7d289b3f",
            "1a6d66db-505d-4da6-90dc-d359235c499a",
            "ccd0d09c-e2ac-40ec-a979-f68ffbbc52f6",
            "27e6e79c-162d-4650-95d2-8a5e4b0a4714",
            "f7a51097-2a50-428a-aecc-ea73453b0587",
            "be811219-7d19-4677-91a2-317efd05007c",
            "c7d4871f-a702-4052-8d3a-4cb621dafae0",
            "c2fa367b-9590-4280-8128-fc8607687b52",
            "fae0c3ef-6b7b-405e-a550-f174a9fd646d",
            "1c2b2c17-9c3b-4fcb-9233-0ff0152fa4e9",
            "bfd51e92-fd76-4a45-8750-7431182c8fbf",
            "554f005c-9bdb-4add-82a7-6e1faa69248a",
            "23589500-cb09-49dc-8203-6cf79ed565d0",
            "73c13b8d-6fc8-4d16-b422-5bd8b2917ff3",
            "7a62b66d-cbc8-49a8-8bb9-28750bd95792",
            "6262c76f-da22-49df-a1be-a96bf1ca6b30",
            "4b30087a-ee8a-4e29-99ba-801c2eb32725",
            "75ba22fc-a1ec-425e-a9b3-0fec47e8351a",
            "7c996b0b-9afc-4e29-b762-3391291ad76c",
            "64528bfa-d84d-4e71-9b69-f0813e7cc225",
            "459837eb-b67e-4dda-bf37-be9c1365f08c",
            "657ec99c-e8f4-4b7c-a765-f96724fdead3",
            "cbd3697b-b4e3-4a52-8660-8619c6ca0130",
            "b2be40f3-f325-4f13-ac90-acdbdac7c935",
            "3bf53da2-d27b-4cd6-bcc1-8f6b1b48ea6d",
            "c2af8ce4-bac5-481e-a8be-d6ea7a536218",
            "64f99e09-7dbd-4762-a7f5-c3a2b9c4fda7",
            "e8d1721a-3f1c-437b-90c7-c9d984693c4a",
            "88d45086-53e3-4a62-9c9e-a2a9f93dabb1",
            "0a34bc31-9472-4cd7-89f1-6a40db8e919c",
            "44b24714-fa38-4ed6-b95d-b0825f177b45",
            "7f28a8c4-bd70-4607-b7b4-a5440fa2de9e",
            "202a1840-8713-4fcb-b44a-27467e1960fc",
            "a88837b4-e3e5-47da-800e-4cd3cbbfc093",
            "67f19d2f-9398-4164-ad01-6fcc897152f3",
            "d1a10a2b-c58c-47a4-b2b5-790b13e9645a",
            "820baa89-3af7-48b9-b49f-5767879bd6f0",
            "23a852b3-0354-4f56-a0b5-c6b55b3ba0c0",
            "19ac6c80-ebd9-4817-b185-859ceef1602d",
            "27707aba-831a-4058-9b3e-1451b047504f",
            "83554b92-e23d-46ad-955e-a0c829d4e3a1",
            "3262fab2-e34c-4410-84e8-5326486bb509",
            "bc142a5c-5105-4960-b7d2-2ce28fda841e",
            "98a1a463-e7ce-4b80-9f54-5a9415dfe669",
            "ab3494e8-c75f-4760-a4d0-a7f323eb84de",
            "f7213f5d-c3b8-4d67-9815-ffed00c9b721",
            "9040faf8-f185-4e17-a956-cac40efc3b2a",
            "757ce7b9-9da8-442a-85fb-0bd14614e091",
            "793b479a-1937-483c-9f88-d0616f875774",
            "23cff744-d3f9-4822-bd9a-af2b544596f8",
            "fd7e1a7c-29dc-42a0-bd4c-338eab35e3f3",
            "8bd24cd3-edf8-4b49-bca0-8713572e4378",
            "c871d56c-3d9d-4808-81b6-31123b7092cb",
            "f4757f43-5e45-4c53-a137-8f169412d2dc",
            "ec197394-5770-4104-98f6-f4c90d65e8cd",
            "5a051df1-0700-4c98-84c9-1a83fd9604ef",
            "edccb345-b48c-4321-b114-6a9d6142012b",
            "b6a62118-6d63-41f3-9d65-dcf120a04038",
            "c961464c-c592-49c3-adc1-939b80c9f291",
            "cb5706b1-4c00-49a5-adad-809b93f49879",
            "a31539d0-c962-4409-8435-8c628327499e",
            "7b88e677-adf4-4939-a41c-37147abbbc87",
            "359c1557-2468-4a99-bb0b-d4d00d9f507a",
            "e52933d5-8c9c-4175-8613-7df2a939a2fc",
            "d48c97da-55dc-4015-a47d-e729f5d2792d",
            "4aba4518-d07f-419a-be2d-fd6936b5d555",
            "e93cf93e-e8ce-4912-9703-f5f9ab620614",
            "9104ebb5-9386-4edb-9748-f0d4ad940ae9",
            "f3a8e5c6-f67c-4251-b9c7-3906b351b56d",
            "669cef3f-155d-421e-901d-2b37060a41c4",
            "bc64d6f3-3d56-4a15-ab4f-830b7026eacc",
            "9eaa2fd9-7bc0-4cfb-820b-e5d05771e078",
            "0c507b8d-96a7-4150-9470-0847b4bbd2c2",
            "51c023d5-159c-4bf4-9096-0d2d830ad2c3",
            "e17aa99c-b711-412e-a44d-b54fcc5936c0",
            "f34d4c18-1f04-4def-8fee-243a3fbec95d",
            "1215536c-3f17-4ecc-af7e-93e0d057ab62",
            "7f0d6954-3393-46a8-94f1-188fd01f8192",
            "003b6f7a-e050-4cb0-84c3-70c4d913fe83",
            "6426059c-77e7-44e1-a409-1484c4a28dc9",
            "79bc746f-4047-4889-a414-375b5d57406c",
            "53adcc96-ba9e-4d40-8342-9fa43aea85b5",
            "69ca15c6-0f9d-478d-84a1-f8eefa83c7b0",
            "7dffd4b7-040e-4cdc-abcb-1d061ff89e28",
            "53113d9a-5028-4e71-b825-1fefa89e5af9",
            "83f8523c-34cc-454c-bdff-1c89270a16d2",
            "867875a2-2d73-4062-9ecb-3dc09879baff",
            "c88e0aea-4c85-4955-9ade-6dcc39d9acb2",
            "c45e26b6-a09b-4ad4-a9c1-c4b944734342",
            "55c488d3-bb95-4993-9fb0-6e78fff6ad5d",
            "8088bf69-1e4c-4171-9f94-f96991b86e4b",
            "86cb67be-0871-4290-8aa4-34a5d353e4e8",
            "92ad470a-c82c-40a6-8cf2-fa85ab617016",
            "80e69cfb-12d2-4e41-a0d4-166215336cc0",
            "f1587a3e-96d3-432b-93f9-b5d21f1e9a61",
            "af6de11c-8b43-4d55-ad4b-b5f8a8bb9c2a",
            "8fbaf05c-2cf1-4cfa-a549-16aaa4fdfc7d",
            "0b58d317-51c3-4aaa-8e0f-5910d27fd91c",
            "b5f6024b-7f92-43c8-9ba1-bda1312cbb8b",
            "c8145ebf-075b-44ce-b672-814438237805",
            "0d6526e3-8a58-4056-a06e-eeea3d010920",
            "551fa48a-4c98-464f-a7e5-f13bb2587495",
            "cf729eaa-c09e-4bf0-9eff-19de1438f6ac",
            "b859210d-1833-4315-b252-1598f3fae80d",
            "6a43e5be-404f-4e6c-846e-b8899091e099",
            "d02ce1d7-663d-42dc-8079-031c080d4c70",
            "a042a964-5a48-4c57-aa33-6876b70a464b",
            "4c00d33b-38b6-4a9b-869d-9119e2058a56",
            "80f6163d-b714-4309-8636-1cf9e5f8b737",
            "4982b7c6-9840-4614-bd82-c6cb6ec39165",
            "79fbe54e-1e56-49ec-9328-7c09194bfad1",
            "7d6e5626-98f0-4607-8bc9-ed90e01b9d8b",
            "c26867ae-2247-4a22-b565-17b32b24a9cb",
            "1e1826bd-d984-4ebf-85ae-2bb501ef27b9",
            "f97b5805-e7ad-4396-92b9-759119a21e3f",
            "5062284d-677b-4d79-97e1-f9738627fb55",
            "f951fc0e-4a44-4950-a7d8-970428641557",
            "ed1e45c7-af56-4071-b184-d1462b5095a7",
            "6ff8b09e-afa8-4439-bea3-7d6ad9cb6f3e",
            "fdf8fd10-d5ef-4ff7-aeea-9b2c9ece23f9",
            "59ca75c9-bbf6-4a52-978e-528d4daacd67",
            "923777a8-e148-4b28-9f97-d6ad56b29922",
            "a75465b1-9e41-4b97-b924-1f7fdb6f69e1",
            "3dc69f66-9555-479a-bb50-815219059ff0",
            "03cf48fa-370e-4f5b-a025-5056345a8790",
            "5d74fd8d-ea20-4a70-835c-f911bb3a7f79",
            "ca12bd3d-5692-4efe-97ab-4ee0808ed209",
            "51498561-4f29-415e-b29c-d3391489076c",
            "55ad4e93-0cce-4780-9547-5d9592dbd02a",
            "e722f72f-5257-4301-99b8-3b05665e2d38",
            "e5a238c9-3852-456d-b8da-293dc26406a7",
            "2f381a1f-3f46-4cde-8c32-392309d88dfe",
            "bd663c6e-9b81-4059-b01b-31975e6890a3",
            "c1c7749e-98e4-4699-aba3-aeffa022cf23",
            "1ab0076e-3727-4cb6-be34-d78ef3192b9c",
            "39d7e300-6345-4059-9065-45747ed69adb",
            "3970f504-dd0e-428d-b369-c7a86a5cc7e0",
            "764a2d1e-e0a2-48ec-9855-712038a3edb1",
            "020014fb-433b-4664-a9d0-44b439a3298a",
            "aa3f38ff-7a52-4cd4-b30e-c9ec2103dad6",
            "3141d30b-f6bf-4dd8-b264-81c4e6895bae",
            "29275d4a-5fec-46ce-8fdf-b3d0fb02163d",
            "94002dd7-a576-4554-94c2-5ac0c59f0613",
            "99faf6b3-ebfa-4ee9-8f24-3f26586df45e",
            "0a84baf0-9af8-45c9-b170-29cc7f98ebaa",
            "cd618de9-9dda-4855-b171-0ef879c6f7d2",
            "019adf69-ba5d-4ca9-bd2b-cc7e18e3b8cb",
            "02234246-b49e-4d59-9848-bbe67c807e89",
            "78fdbee8-5264-4dbb-82ae-b5c65d0522ea",
            "c2071344-1596-4548-a5f0-2a742418c7d8",
            "ae166c45-5fcf-4575-a198-20bb66b291dc",
            "8eb69951-42d0-491f-b427-2bfe8cae6afb",
            "2f13b5a9-7f61-4215-8521-48da50204ca9",
            "dbd30c2e-4672-4661-8715-aa0059aa7970",
            "1eaaebe6-6275-49b3-9253-b8ca6920f0c8",
            "9113c826-dc56-4b7f-b8e3-840267eb9fb4",
            "9ad3f994-10ad-4332-9791-a18b33636132",
            "5642b4c6-d2b5-4cdc-9faf-6dc065fe865a",
            "bd068c05-44e3-4e5e-bc81-670e82fdb118",
            "5a8c51b8-a81c-4745-ba07-c36decebdd84",
            "95eb8204-feee-4c57-835f-af243f21cbfc",
            "ee205b95-c8e9-4fbe-9f23-c495da5e6775",
            "06a4a499-3932-4871-b32d-fe2292ce8b46",
            "388db805-0925-4cee-b6fe-d463c93b35b6",
            "24fdc57b-7c72-4bbb-b3b1-cdddf895fe59",
            "3a427862-2fde-48aa-a580-fef9ba5267e3",
            "12b60b08-a660-4d83-b733-19a51ce51454",
            "64aaab2b-b937-4d10-a6d5-ac46116c2e0c",
            "a14b86a6-e41b-400b-9302-f29661d0dda4",
            "fdc7568f-f815-4b19-86ed-faa9977a2438",
            "d624abdc-1b4e-40ac-897a-73dc57b77063",
            "8ba5ef0e-d0ba-466e-a550-5c47d8709968",
            "1cf64c79-0306-498a-bd93-8c10c0f700fe",
            "54316d21-e610-454a-aaa0-8537ff1770ad",
            "8decd5db-0f9b-4d57-8ef4-8fd93b186ba6",
            "4362239c-d41e-4323-ad06-830afc0507a6",
            "45447e4e-8ade-44f4-a84b-77f814385c35",
            "e3ee1a35-77b6-435b-b13c-e02c9f4be537",
            "86dc5235-fe9e-4db3-a30c-6986c47bc13c",
            "850f7609-913b-457c-b903-c30aa15c19e6",
            "02a6e650-3490-4462-9e6f-c14f2eddaee7",
            "8d478dcf-7bfd-4057-907a-1de8c8f412ed",
            "79bb8a61-afd6-46dd-8e51-3171a8158b84",
            "c31ab7d4-6ede-4297-86f4-9fa1bb3c21cc",
            "12ff2e3f-cde5-41f4-ae93-937be604f238",
            "e8e49877-f80d-439a-bfaf-581a5e479cf6",
            "a266622d-19af-43f0-91e9-0fc6f7523406",
            "e2356b59-724e-482a-ab10-471ac2a3c9dc",
            "2dc8ae48-e463-41b5-b44e-9387a182e787",
            "b1fe7bc9-4877-4648-8126-9bd89213be94",
            "3e1ccd70-f2c1-4144-ad6e-33db97935399",
            "0be18fc3-a92a-4115-831b-883f263ce842",
            "cc8751ae-d653-4f67-8da8-09d28e6538da",
            "ed978051-d004-40e4-b90c-245acc4ec5e5",
            "454c30a5-3bdf-4e1f-aec0-23da76c545d1",
            "beb1dd1f-ef96-4288-a373-d23cdce099d2",
            "4fa9cbe7-7b35-42c9-b6d7-033730c9b53c",
            "d32ab7e3-8284-4898-bf91-a4f03aa0223a",
            "d95d155b-aa6c-4922-845c-81a5f159e147",
            "d614be09-26a4-4df1-92d9-885f0752f89e",
            "f5e47e90-8926-4347-a601-bba46fa20d64",
            "05449556-293e-4980-a7c0-2e1ab29475ee",
            "01d14b76-a58b-4b7e-84e2-41710ebbd417",
            "16dc36bd-54f8-48b6-b06c-777b8af5e566",
            "bfbeee00-be7b-4972-8e60-76da7a26ee2a",
            "cf144d69-b4a2-405e-989e-b44b68a9a765",
            "741992ec-a8c7-4af5-a6cc-c8fdf68b8351",
            "b96c8ab4-692e-4fc0-8a35-a776c3f18866",
            "f710c50d-e1d1-48c8-b6f9-a94cd8541fe5",
            "9e12b8a2-c75b-4c2a-91ca-5754e0359b36",
            "fc7c81e9-ec72-4ef8-bffb-3ec55d67bd74",
            "660aea28-fd92-4926-a2ce-d0209667d374",
            "0569cb06-8490-41c4-a937-7699c73827ae",
            "eab0a632-a9f4-4fae-a856-fe5fd6545aa6",
            "dc7c6829-50ec-4e9a-9611-476894bb91bc",
            "e647796b-ca29-4f74-83a1-5f1c915abcce",
            "1d5c9e75-9d9c-4c41-a28f-22949855242d",
            "62e2e5e2-4fab-45f5-b015-42d7a4754477",
            "3ad40de5-010b-404f-9e2e-cd58d3182b66",
            "5c313f3e-ff12-4783-9286-44177a892838",
            "22808322-c26f-4637-977c-372a260ec8de",
            "cd99f9a4-6666-486b-9d1a-6659979d6859",
            "8f9ce971-30ba-4dc9-84d2-513f7d107dd3",
            "bd2b04e7-c63b-48c5-94f9-5375102168cd",
            "28f3d079-e4b1-4c13-b912-0e999f1ec14a",
            "5e842891-f2ba-48a2-a3c0-ff67fd583bc8",
            "6f694ddd-d355-4ed2-aaec-0473699041ff",
            "f6bcbee2-69cd-41b9-901d-f0ceb711f50c",
            "a6bd72c9-3753-461e-81cb-647065ad1e08",
            "d73bb373-a8d2-434f-89c3-5c83b65e4df0",
            "7d487de2-ddd6-4707-b0b9-28279a07f67a",
            "fb8e1449-e475-4174-b4ce-5173e819e23a",
            "24a99ae8-f69c-4093-9f7f-7c129dc15fcb",
            "4f05b9fe-577f-4f25-b8ed-20d2bf65c324",
            "9722c270-f0f4-4777-b7cd-b8174bb7e15b",
            "fd196fa3-0642-400b-aca0-6e9e21534f6a",
            "e3460ba8-ed05-4d09-b541-7660b41bb5f5",
            "b22304ff-38ff-4caa-94dd-5c53618dcb94",
            "e51e72a7-0942-4582-833a-fc64d17c1f19",
            "44b1f6b7-950b-4e24-aae8-05a9b070aa60",
            "dec0231a-fca5-4f7e-b265-d3a7fb50aac8",
            "194c50be-b14c-4c8d-bfb5-eddd36c78337",
        };

        private readonly ManualApprovalRecordRepository Repository;

        private readonly IRMManualApproveHistoryDao ManualApproveHistoryDao = new RMManualApproveHistoryDao();

        public ManualSepCIUpgradeOnlyForClp()
        {
            try
            {
                Repository = new ManualApprovalRecordRepository();
            }
            catch (Exception e)
            {
                Logger.Error($"[Sep CI Upgrade Only For Clp ERROR] An error occurred while execute sep ci upgrade. Error: {e}");
            }
        }

        public async System.Threading.Tasks.Task UpgradeAsync()
        {
            // CLP Tenant
            if (TenantLocalValue.LogonGroupId == "f4a831cc-1d2e-4a2a-ba9a-33e4de7f72a5")
            //if (TenantLocalValue.LogonGroupId == "35226de4-9d1c-44df-8dd9-2b419109e93c")
            {
                await RemoveApprovalFailedItemsAsync();
                RemoveAutoRejectedItems();
            }

            //RemoveManualWaitingHistoryItems();
        }

        private void RemoveAutoRejectedItems()
        {
            try
            {
                Logger.Info($"[Sep CI Upgrade Only For Clp RemoveAutoRejectedItems Start INFO] Start process remove approval failed items.");

                var effectionItemsCount = ManualApproveHistoryDao.ExecuteSqlCommand((schemaName) =>
                {
                    return $"UPDATE [{SecurityUtils.SanitizeSQLSchemaName(schemaName)}].[RMManualApproveHistories] SET IsRemoved = 1 Where ApprovedStatus = 4 AND ApprovedBy = 8";
                });

                Logger.Info($"[Sep CI Upgrade Only For Clp RemoveApprovalFailedItems Succeed INFO] Total removed auto rejected items count: [{effectionItemsCount}].");
            }
            catch (Exception e)
            {
                Logger.Error($"[Sep CI Upgrade Only For Clp RemoveAutoRejectedItems ERROR] An error occurred while execute sep ci upgrade. Error: {e}");
            }
        }

        //638007516000000000 2022/10/07 15:00:00
        private async System.Threading.Tasks.Task RemoveApprovalFailedItemsAsync()
        {
            try
            {
                Logger.Info($"[Sep CI Upgrade Only For Clp RemoveApprovalFailedItems Start INFO] Start process remove approval failed items.");

                var instanceIds = ApprovedIntanceIds.Distinct().Select(item => new Guid(item)).ToList();
                Logger.Info($"[Sep CI Upgrade Only For Clp RemoveApprovalFailedItems INFO] Approved workflow instance ids count [{instanceIds.Count}].");
                var items = await Repository.QueryItemsAsync(item => instanceIds.Contains(item.ManualWorkflowInstanceId));
                Logger.Info($"[Sep CI Upgrade Only For Clp RemoveApprovalFailedItems INFO] Approved workflow instance ids related cosmos db items count [{items.Count}].");

                var fullPaths = items.Select(item => item.ManualFullPath).ToList();
                var totalEffectItemsCount = 0;

                for (var i = 0; i < fullPaths.Count; i += 20)
                {
                    var needUpdateFullPaths = fullPaths.Skip(i).Take(20);
                    string conditions = DatabaseUtility.BuildInClause(needUpdateFullPaths, out List<SqlParameter> parameters);
                    var effectItemsCount = ManualApproveHistoryDao.ExecuteSqlCommand((schemaName) =>
                    {
                        return $"UPDATE [{SecurityUtils.SanitizeSQLSchemaName(schemaName)}].[RMManualApproveHistories] SET IsRemoved = 1 Where FullPath IN {conditions} AND ArchivedTime = 0 AND ActionTime < 638007516000000000 ";
                    }, parameters);

                    Logger.Info($"[Sep CI Upgrade Only For Clp RemoveApprovalFailedItems INFO] Current full paths count [{needUpdateFullPaths}] effect items count: [{effectItemsCount}].");
                    totalEffectItemsCount += effectItemsCount;
                }

                Logger.Info($"[Sep CI Upgrade Only For Clp RemoveApprovalFailedItems Succeed INFO] Total need removed items count: [{totalEffectItemsCount}].");
            }
            catch (Exception e)
            {
                Logger.Error($"[Sep CI Upgrade Only For Clp RemoveApprovalFailedItems ERROR] An error occurred while execute sep ci upgrade. Error: {e}");
            }
        }

        /*private void RemoveManualWaitingHistoryItems()
        {
            try
            {
                Logger.Info($"[Sep CI Upgrade RemoveManualWaitingHistoryItems Start INFO] Start process remove manual waiting history failed items.");

                var effectionItemsCount = ManualApproveHistoryDao.ExecuteSqlCommand((schemaName) =>
                {
                    return $"DELETE FROM [{schemaName}].[RMManualApproveHistories] Where ApprovedStatus = 1";
                });

                Logger.Info($"[Sep CI Upgrade RemoveApprovalFailedItems Succeed INFO] Total removed manual waiting history items count: [{effectionItemsCount}].");
            }
            catch (Exception e)
            {
                Logger.Error($"[Sep CI Upgrade RemoveManualWaitingHistoryItems ERROR] An error occurred while execute sep ci upgrade. Error: {e}");
            }
        }*/
    }
}
