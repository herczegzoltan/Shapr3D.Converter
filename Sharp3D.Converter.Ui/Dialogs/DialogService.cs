using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Sharp3D.Converter.Ui.Dialogs
{
    public class DialogService : ContentDialog, IDialogService
    {
        public async Task ShowExceptionModalDialog(Exception ex, string description = null)
        {
            try
            {
                Title = "Exception thrown";
                Content = $"{ex.Message} {description}";
                CloseButtonText = "Ok";

                await ShowAsync();
            }
            catch (Exception)
            {
                throw ex;
            }
        }

        public async Task<bool?> ShowBlockingQuestionModalDialog(string tilte, string description, string primary = "Yes", string secondary = "No")
        {
            try
            {
                Title = tilte;
                Content = description;
                PrimaryButtonText = primary;
                CloseButtonText = secondary;

                var result = await ShowAsync();

                return result == ContentDialogResult.Primary;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task ShowOkModalDialog(string tilte, string description)
        {
            try
            {
                Title = tilte;
                Content = description;
                CloseButtonText = "Ok";

                await ShowAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
