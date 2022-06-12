using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Sharp3D.Converter.Ui.Dialogs
{
    public interface IDialogService
    {
        Task<bool?> ShowBlockingQuestionModalDialog(string tilte, string description, string primary = "Yes", string secondary = "No");
        Task ShowExceptionModalDialog(Exception ex, string description = null);
    }

    public class DialogService : ContentDialog, IDialogService
    {
        public async Task ShowExceptionModalDialog(Exception ex, string description = null)
        {
            try
            {
                Title = "Exception thrown";
                Content = description;
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
    }
}
