using Shapr3D.Converter.ViewModels;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Shapr3D.Converter.Controls
{
    // TODO: Implement as a templated control instead of UserControl
    public sealed partial class ConverterStateButton : UserControl
    {
        public ConverterStateButton()
        {
            InitializeComponent();
        }

        public ConversionInfo.ConversionState ConversionState
        {
            get { return (ConversionInfo.ConversionState)GetValue(ConversionStateProperty); }
            set { SetValue(ConversionStateProperty, value); }
        }

        public static readonly DependencyProperty ConversionStateProperty =
            DependencyProperty.Register(nameof(ConversionState), typeof(ConversionInfo.ConversionState), typeof(ConverterStateButton), new PropertyMetadata(ConversionInfo.ConversionState.NotStarted));

        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(nameof(Progress), typeof(double), typeof(ConverterStateButton), new PropertyMetadata(0.0));


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(ConverterStateButton), new PropertyMetadata(""));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ConverterStateButton), new PropertyMetadata(null));

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(ConverterStateButton), new PropertyMetadata(0));
    }
}
