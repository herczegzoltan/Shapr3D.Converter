using System;
using System.Threading.Tasks;

namespace Sharp3D.Converter.Ui.Dialogs
{
    public interface IDialogService
    {
        Task<bool?> ShowBlockingQuestionModalDialog(string tilte, string description, string primary = "Yes", string secondary = "No");
        Task ShowExceptionModalDialog(Exception ex, string description = null);
        Task ShowOkModalDialog(string tilte, string description);
    }
}
