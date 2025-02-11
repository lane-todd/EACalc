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
                                   "Please enter a Parcel Number.",
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

            try
            {
                using (var connection = new OdbcConnection(connectionString))
                {
                    //await connection.OpenAsync();
                    using (var command = new OdbcCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ParcelNumber", parcelNumber);

                        // Display the constructed SQL query for debugging
                        string fullQuery = command.CommandText;
                        fullQuery = fullQuery.Replace("@ParcelNumber", "'" + parcelNumber + "'"); // Replace the placeholder with the actual value
                        await DisplayAlert("SQL Query", fullQuery, "OK");

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            // ... (rest of your code) ...
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
