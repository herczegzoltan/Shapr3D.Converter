using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Sharp3D.Converter.Ui.Dialogs
{
    // Referenced https://docs.microsoft.com/en-us/uwp/api/windows.ui.popups.messagedialog?view=winrt-22621
    public class DialogService : IDialogService
    {
        public async Task ShowExceptionModalDialog(Exception ex, string description = null, string buttonContent = "Ok")
        {
            try
            {
                var title = "Exception thrown";
                var content = $"{ex.Message} {description}";
                var cancelCommand = new UICommand(buttonContent, cmd => {});
                
                var dialog = new MessageDialog(content, title);
                dialog.Options = MessageDialogOptions.None;
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 0;

                if (cancelCommand != null)
                {
                    dialog.Commands.Add(cancelCommand);
                    dialog.CancelCommandIndex = (uint)dialog.Commands.Count - 1;
                }

                _ = await dialog.ShowAsync();
            }
            catch (Exception)
            {
                throw ex;
            }
        }

        public async Task<bool?> ShowBlockingQuestionModalDialog(string title, string description, string primary = "Yes", string secondary = "No")
        {
            try
            {
                var primaryCommand = new UICommand(primary, cmd => { });
                var secondaryCommand = new UICommand(secondary, cmd => { });

                var dialog = new MessageDialog(description, title);
                dialog.Options = MessageDialogOptions.None;
                dialog.Commands.Add(primaryCommand);
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 0;

                if (secondaryCommand != null)
                {
                    dialog.Commands.Add(secondaryCommand);
                    dialog.CancelCommandIndex = (uint)dialog.Commands.Count - 1;
                }

                var command = await dialog.ShowAsync();

                return command == primaryCommand;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task ShowOkModalDialog(string title, string description, string buttonContent = "Ok")
        {
            try
            {
                var cancelCommand = new UICommand(buttonContent, cmd => { });
                
                var dialog = new MessageDialog(description, title);
                dialog.Options = MessageDialogOptions.None;
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 0;

                if (cancelCommand != null)
                {
                    dialog.Commands.Add(cancelCommand);
                    dialog.CancelCommandIndex = (uint)dialog.Commands.Count - 1;
                }

                _ = await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
