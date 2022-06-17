using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Sharp3D.Converter.Ui.Dialogs
{
    /// <inheritdoc/>
    public class DialogService : IDialogService
    {
        public async Task ShowExceptionModalDialog(Exception ex, string description = null, string buttonContent = "Ok")
        {
            var title = "Exception thrown";
            var content = $"{ex.Message} {description}";
            var cancelCommand = new UICommand(buttonContent, cmd => { });

            var dialog = new MessageDialog(content, title)
            {
                Options = MessageDialogOptions.None,
                DefaultCommandIndex = 0,
                CancelCommandIndex = 0
            };

            if (cancelCommand != null)
            {
                dialog.Commands.Add(cancelCommand);
                dialog.CancelCommandIndex = (uint)dialog.Commands.Count - 1;
            }

            _ = await dialog.ShowAsync();
        }

        public async Task<bool?> ShowBlockingQuestionModalDialog(string title, string description, string primary = "Yes", string secondary = "No")
        {
            var primaryCommand = new UICommand(primary, cmd => { });
            var secondaryCommand = new UICommand(secondary, cmd => { });

            var dialog = new MessageDialog(description, title)
            {
                Options = MessageDialogOptions.None,
                DefaultCommandIndex = 0,
                CancelCommandIndex = 0
            };
            dialog.Commands.Add(primaryCommand);

            if (secondaryCommand != null)
            {
                dialog.Commands.Add(secondaryCommand);
                dialog.CancelCommandIndex = (uint)dialog.Commands.Count - 1;
            }

            var command = await dialog.ShowAsync();

            return command == primaryCommand;
        }

        public async Task ShowOkModalDialog(string title, string description, string buttonContent = "Ok")
        {
            var cancelCommand = new UICommand(buttonContent, cmd => { });

            var dialog = new MessageDialog(description, title)
            {
                Options = MessageDialogOptions.None,
                DefaultCommandIndex = 0,
                CancelCommandIndex = 0
            };

            if (cancelCommand != null)
            {
                dialog.Commands.Add(cancelCommand);
                dialog.CancelCommandIndex = (uint)dialog.Commands.Count - 1;
            }

            _ = await dialog.ShowAsync();
        }
    }
}
