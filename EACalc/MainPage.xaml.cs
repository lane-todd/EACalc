using System.Text.RegularExpressions;
using System.Data.Odbc;
using System.Data;
namespace EACalc

{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void SearchButtonClicked(object sender, EventArgs e)
        {
            string parcelNumber = ParcelNumberEntry.Text;

            if (string.IsNullOrEmpty(parcelNumber))
            {
                // Display an error message (e.g., using a message dialog)
                await DisplayAlert("Error",
                                   "Please enter a 10-digit Parcel Number.",
                                   "OK");
                ParcelNumberEntry.Text = "";
                return;
            }
            // Validate parcel number (exactly 10 digits and only digits)
            if (!Regex.IsMatch(parcelNumber, @"^\d{10}$"))
            {
                // Display an error message (e.g., using a message dialog)
                await DisplayAlert("Error", "Invalid Parcel Number. Please enter a 10-digit number.", "OK");
                ParcelNumberEntry.Text = "";
                return;
            }

            // Sanitize the parcel number for SQL injection prevention
            parcelNumber = $"{parcelNumber}";

            string connectionString = "DSN=asrall;Description=asrall;DATABASE=ASR;UID=asrall;PWD=asrall;";

            string sqlQuery = @"
                    WITH RankedParcels AS (
                        SELECT 
                            CUSTOM.VlcResParcels.IMPNO AS IMPNO1, 
                            CUSTOM.VlcResParcels.YRBLT,
                            CUSTOM.VlcResParcels.ADJYRBLT, 
                            CUSTOM.VlcResParcels.SF AS SF1,
                            CUSTOM.VlcResParcels.BSMNTSF, 
                            CUSTOM.VlcResParcels.PARCELNO,
                            ROW_NUMBER() OVER (PARTITION BY CUSTOM.VlcResParcels.IMPNO ORDER BY CUSTOM.VlcResParcels.IMPNO ASC) AS rn
                        FROM 
                            CUSTOM.VlcResParcels
                        WHERE 
                            CUSTOM.VlcResParcels.PARCELNO = @ParcelNumber
                    )
                    SELECT IMPNO1, YRBLT, ADJYRBLT, SF1, BSMNTSF, PARCELNO
                    FROM RankedParcels
                    WHERE rn = 1";

            string fullQuery = sqlQuery.Replace("@ParcelNumber", "'" + parcelNumber + "'"); // Replace the placeholder with the actual value

            try
            {
                using (var connection = new OdbcConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new OdbcCommand(fullQuery, connection))
                    {
                        // await DisplayAlert("SQL Query", fullQuery, "OK");

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // Get column ordinals 
                                int yrbltIndex = reader.GetOrdinal("YRBLT");
                                int adjyrbltIndex = reader.GetOrdinal("ADJYRBLT");
                                int sf1Index = reader.GetOrdinal("SF1");
                                int bsmntsfIndex = reader.GetOrdinal("BSMNTSF");
                                // set values, check if null. if null, default to 0
                                int yrblt = reader.IsDBNull(yrbltIndex) ? 0 : (int)reader.GetValue(yrbltIndex);
                                int adjyrblt = reader.IsDBNull(adjyrbltIndex) ? 0 : (int)reader.GetValue(adjyrbltIndex);
                                decimal sf1 = reader.IsDBNull(sf1Index) ? 0 : (decimal)reader.GetValue(sf1Index);
                                decimal bsmntsf = reader.IsDBNull(bsmntsfIndex) ? 0 : (decimal)reader.GetValue(bsmntsfIndex);

                                YearBuiltEntry.Text = yrblt;
                                AdjustedYearBuiltEntry.Text = adjyrblt;
                                SquareFootageEntry.Text = sf1;
                                BasementSqFtEntry.Text = bsmntsf;
                            }
                            else
                            {
                                await DisplayAlert("Information", "No data found for the given Parcel Number.", "OK");
                                // Clear existing entry fields (optional)
                                YearBuiltEntry.Text = string.Empty;
                                AdjustedYearBuiltEntry.Text = string.Empty;
                                SquareFootageEntry.Text = string.Empty;
                                NewSquareFootageEntry.Text = string.Empty;
                                BasementSqFtEntry.Text = string.Empty;
                            }
                        }
                    }
                }
            }
            catch (OdbcException ex)
            {
                await DisplayAlert("Error", $"OdbcException: {ex.Message}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        
        // Example: Display the sanitized parcel number (for demonstration)
        await DisplayAlert("Parcel Number", $"Sanitized Parcel Number: {parcelNumber}", "OK");
        }










        private async void ShowKitchenRemodelInfo(object sender, EventArgs e)
        {
            await DisplayAlert("Kitchen Remodel Info",
                               "Total kitchen remodel should include new (not just painted) cabinets, countertops, flooring, etc.",
                               "OK");
        }
        private async void ShowBathroomRemodelInfo(object sender, EventArgs e)
        {
            await DisplayAlert("Bathroom Remodel Info",
                               "Total bathroom remodel should include new tile, flooring, cabinets, countertops, etc. If multiple bahtrooms in the home, this should include remodeling of all bathrooms.",
                               "OK");
        }
        private async void ShowCosmeticRemodelInfo(object sender, EventArgs e)
        {
            await DisplayAlert("Cosmetic Remodel Info",
                               "Cosmetic includes replacing flooring throughout, trim, hardware, painting, etc. The property is probably being refreshed so that it shows well.",
                               "OK");
        }
        private async void ShowRoofRemodelInfo(object sender, EventArgs e)
        {
            await DisplayAlert("Roof Remodel Info",
                               "Roof includes replacing any or all part(s) of the existing roof.",
                               "OK");
        }
        private async void ShowElectricalAndPlumbingRemodelInfo(object sender, EventArgs e)
        {
            await DisplayAlert("Electrical & Plumbing Remodel Info",
                               "Full electrical and plumbing replcement (not limited to 1 or 2 rooms). Likely includes electrical panel, hot water heater and furnace. This rarely happens and typically occurs in old houses that undergo significant renovation.",
                               "OK");
        }
        private async void ShowBasementRemodelInfo(object sender, EventArgs e)
        {
            await DisplayAlert("Basement Finish Remodel Info",
                               "Basement finish remodels are an approximation of how much basement squarefootage becomes finished, liveable space as the result of renovations - expressed as a percentage of total basement squarefootage.",
                               "OK");
        }
        private async void ShowNewSquareFootageInfo(object sender, EventArgs e)
        {
            await DisplayAlert("New Square Footage Info",
                               "Additional square footage being added on to an existing improvement in the form of an addition. If no additions are being built, value defaults to 0 sqft.",
                               "OK");
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }

}
