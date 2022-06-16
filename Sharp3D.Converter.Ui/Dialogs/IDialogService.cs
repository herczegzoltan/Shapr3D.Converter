using System;
using System.Threading.Tasks;

namespace Sharp3D.Converter.Ui.Dialogs
{
    public interface IDialogService
    {
        /// <summary>
        /// Show two options dialogs by default Yes/No.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="description">The description of the dialog.</param>
        /// <param name="primary">The content of the primary button by default it's "Yes".</param>
        /// <param name="secondary">The content of the secondary button by default it's "No".</param>
        /// <returns>Returns true if the primary is selected otherwise false</returns>
        Task<bool?> ShowBlockingQuestionModalDialog(string title, string description, string primary = "Yes", string secondary = "No");

        /// <summary>
        /// Show a specific exception in dialog.
        /// </summary>
        /// <param name="ex">The instance of the excpetion.</param>
        /// <param name="description">The description of the dialog by default it's message of the exception.</param>
        /// <param name="buttonContent">The content of the close button by default it's "Ok".</param>
        /// <returns>Close the dialog and representing the asynchronous operation.</returns>
        Task ShowExceptionModalDialog(Exception ex, string description = null, string buttonContent = "Ok");

        /// <summary>
        /// Show an information dialog.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="description">The description of the dialog.</param>
        /// <param name="buttonContent">The content of the close button by default it's "Ok".</param>
        /// <returns>Close the dialog and representing the asynchronous operation.</returns>
        Task ShowOkModalDialog(string tilte, string description, string buttonContent = "Ok");
    }
}
