using System;
using System.Threading.Tasks;

namespace Sharp3D.Converter.Ui.Dialogs
{
    public interface IDialogService
    {
        Task<bool?> ShowBlockingQuestionModalDialog(string title, string description, string primary = "Yes", string secondary = "No");
        Task ShowExceptionModalDialog(Exception ex, string description = null, string buttonContent = "Ok");
        Task ShowOkModalDialog(string tilte, string description, string buttonContent = "Ok");
    }
}
