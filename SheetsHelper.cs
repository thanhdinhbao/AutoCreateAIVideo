using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

namespace AutoCreateAIVideo
{
    public class SheetsHelper
    {
        private const string SpreadsheetId = "1u1F5OpkfcLZR9-sH9LP95H332KRPnjt8FtoT38Ycwuo"; // <== Thay bằng ID thật
        private const string SheetName = "Sheet1";
        private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private SheetsService _sheetsService;

        public SheetsHelper()
        {
            InitializeSheetsService();
        }

        private void InitializeSheetsService()
        {
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string tokenPath = "token.json";

                var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(tokenPath, true)
                ).Result;

                _sheetsService = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Prompt Tool",
                });
            }
        }

        public async Task<List<(int RowIndex, string Prompt)>> GetInprocessPromptsAsync()
        {
            var range = $"{SheetName}!A2:D";
            var request = _sheetsService.Spreadsheets.Values.Get(SpreadsheetId, range);
            var response = await request.ExecuteAsync();

            var results = new List<(int, string)>();
            if (response.Values == null) return results;

            int baseRow = 2;
            for (int i = 0; i < response.Values.Count; i++)
            {
                var row = response.Values[i];
                if (row.Count >= 2 && row[1].ToString().Trim().ToLower() == "inprocess")
                {
                    string prompt = row[0].ToString();
                    results.Add((baseRow + i, prompt));
                }
            }

            return results;
        }

        public async Task UpdatePromptRowAsync(int rowIndex, string imageUrl, string videoUrl = null)
        {
            var values = new List<IList<object>>
        {
            new List<object> { "", "Processed", imageUrl, videoUrl ?? "" } // cột A giữ nguyên
        };

            var valueRange = new ValueRange
            {
                Values = values
            };

            string range = $"{SheetName}!A{rowIndex}:D{rowIndex}";
            var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, SpreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            await updateRequest.ExecuteAsync();
        }
    }
}
