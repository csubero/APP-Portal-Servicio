using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.Controls
{
    public class ImageButton : Image
    {
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ImageButton), null);

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ImageButton), null);

        public static readonly BindableProperty IsBusyProperty =
            BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(ImageButton), false,BindingMode.OneWay,null, IsBusyPropertyChanged);

        public static readonly BindableProperty IsActivatedProperty =
           BindableProperty.Create(nameof(IsActivated), typeof(bool), typeof(ImageButton), true,BindingMode.OneWay,null,IsActivatedPropertyChanged);

        public ImageButton()
        {
            Initialize();
        }
        public bool IsActivated
        {
            get { return (bool)GetValue(IsActivatedProperty); }
            set { SetValue(IsActivatedProperty, value); Opacity = IsActivated ? 1f : .5f; }
        }
        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        private ICommand TransitionCommand
        {
            get
            {
                return new Command(async () =>
                {
                    if (!IsActivated || IsBusy)
                        return;
                    AnchorX = 0.48;
                    AnchorY = 0.48;
                    await this.ScaleTo(0.8, 50, Easing.Linear);
                    await Task.Delay(100);
                    await this.ScaleTo(1, 50, Easing.Linear);
                    if (Command != null)
                    {
                        Command.Execute(CommandParameter);
                    }
                });
            }
        }

        public void Initialize()
        {
            GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = TransitionCommand
            });
        }

        private static void IsBusyPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var imagebutton = (ImageButton)bindable;
            imagebutton.Opacity = imagebutton.IsActivated && !(bool)newValue ? 1f : .5f;
        }

        private static void IsActivatedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var imagebutton = (ImageButton)bindable;
            imagebutton.Opacity = imagebutton.IsActivated? 1f : .5f;
        }
    }
}
