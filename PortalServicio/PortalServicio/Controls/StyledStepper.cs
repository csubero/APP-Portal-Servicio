using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace PortalServicio.Controls
{
    public class StyledStepper : ContentView
    {
        #region Backing Fields
        public static readonly BindableProperty MinimumProperty = BindableProperty.Create(nameof(Minimum), typeof(int), typeof(StyledStepper), 0);
        public static readonly BindableProperty MaximumProperty = BindableProperty.Create(nameof(Maximum), typeof(int), typeof(StyledStepper), 100);
        public static readonly BindableProperty ValueProperty = BindableProperty.Create(nameof(Value), typeof(int), typeof(StyledStepper), 0, defaultBindingMode: BindingMode.TwoWay, propertyChanged: ValuePropertyChanged);
        public static readonly BindableProperty StepProperty = BindableProperty.Create(nameof(Step), typeof(int), typeof(StyledStepper), 1);
        public static readonly BindableProperty IsMAXEnableProperty = BindableProperty.Create(nameof(IsMAXEnable), typeof(bool), typeof(StyledStepper), false);
        public static readonly BindableProperty FixedStepProperty = BindableProperty.Create(nameof(FixedStep), typeof(int), typeof(StyledStepper), 10);
        public static readonly BindableProperty StepperColorProperty = BindableProperty.Create(nameof(StepperColor), typeof(Color), typeof(StyledStepper), Color.Black, defaultBindingMode: BindingMode.TwoWay, propertyChanged: StepperColorPropertyChanged);
        public static readonly BindableProperty SecondaryColorProperty = BindableProperty.Create(nameof(SecondaryColor), typeof(Color), typeof(StyledStepper), Color.Gray, defaultBindingMode: BindingMode.TwoWay, propertyChanged: SecondaryColorPropertyChanged);
        public static readonly BindableProperty AuxCommandProperty = BindableProperty.Create(nameof(AuxCommand), typeof(ICommand), typeof(StyledStepper), null);
        #endregion

        #region Properties
        private ExtendedButton _leftbutton;
        private ExtendedButton _centerbutton;
        private ExtendedButton _rightbutton;
        private ExtendedButton _topbutton;
        public int Step
        {
            get { return (int)GetValue(StepProperty); }
            set { SetValue(StepProperty, value); }
        }
        public int FixedStep
        {
            get { return (int)GetValue(FixedStepProperty); }
            set { SetValue(FixedStepProperty, value); ((Button)((AbsoluteLayout)Content).Children[2]).Text = IsMAXEnable? "MAX": "+" + value; }
        }
        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public Color StepperColor
        {
            get { return (Color)GetValue(StepperColorProperty); }
            set { SetValue(StepperColorProperty, value); }
        }
        public Color SecondaryColor
        {
            get { return (Color)GetValue(SecondaryColorProperty); }
            set { SetValue(SecondaryColorProperty, value); }
        }
        public ICommand AuxCommand
        {
            get { return (ICommand)GetValue(AuxCommandProperty); }
            set { SetValue(AuxCommandProperty, value); }
        }
        public bool IsMAXEnable
        {
            get { return (bool)GetValue(IsMAXEnableProperty); }
            set { SetValue(IsMAXEnableProperty, value); ((Button)((AbsoluteLayout)Content).Children[2]).Text = value?"MAX":"+" + FixedStep; }
        }
        #endregion

        #region Constructor
        public StyledStepper()
        {
            AddStyles();
            AbsoluteLayout layout = new AbsoluteLayout();
            _leftbutton = new ExtendedButton()
            {
                Text = $"-",
                ClassId = $"1",
                Style = Resources["unSelectedStyle"] as Style,
                WidthRequest = 50,
                TextSize = 15,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Start,
            };
            AbsoluteLayout.SetLayoutBounds(_leftbutton, new Rectangle(0, 40, 50, 40));
            _centerbutton = new ExtendedButton()
            {
                Text = MinMaxValueObtain(Minimum, Maximum, Value).ToString(),
                ClassId = $"2",
                Style = Resources["selectedStyle"] as Style,
                WidthRequest = 60,
                HeightRequest = 60,
                CornerRadius = 30,
                HorizontalTextAlignment = TextAlignment.Center,
                TextSize = 30,
                FontAttributes = FontAttributes.Bold,
            };
            AbsoluteLayout.SetLayoutBounds(_centerbutton, new Rectangle(30, 30, 80, 60));
            _rightbutton = new ExtendedButton()
            {
                Text = $"+",
                ClassId = $"1",
                Style = Resources["unSelectedStyle"] as Style,
                WidthRequest = 50,
                TextSize = 15,
                HorizontalTextAlignment = TextAlignment.End,
                FontAttributes = FontAttributes.Bold,
            };
            AbsoluteLayout.SetLayoutBounds(_rightbutton, new Rectangle(90, 40, 50, 40));
            _topbutton = new ExtendedButton()
            {
                Text = IsMAXEnable?"MAX":"+"+FixedStep,
                ClassId = $"1",
                Style = Resources["auxButtonStyle"] as Style,
                WidthRequest = 60,
                HeightRequest = 70,
                TextSize = 15,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Start,
                FontAttributes = FontAttributes.Bold,
            };
            AbsoluteLayout.SetLayoutBounds(_topbutton, new Rectangle(40, 0, 60, 70));
            _rightbutton.Clicked += Right_Click;
            _leftbutton.Clicked += Left_Click;
            _topbutton.Clicked += Top_Click;
            layout.Children.Add(_leftbutton);
            layout.Children.Add(_rightbutton);
            layout.Children.Add(_topbutton);
            layout.Children.Add(_centerbutton);
            Content = layout;
        }

        private async void Center_Click(object sender, EventArgs e)
        {
            AnchorX = 0.48;
            AnchorY = 0.48;
            await _rightbutton.ScaleTo(0.8, 50, Easing.Linear);
            AuxCommand?.Execute(null);
            await Task.Delay(100);
            await _rightbutton.ScaleTo(1, 50, Easing.Linear);
        }

        private async void Right_Click(object sender, EventArgs e)
        {
            AnchorX = 0.48;
            AnchorY = 0.48;
            await _rightbutton.ScaleTo(0.8, 50, Easing.Linear);
            if (Value < Maximum)
                Value += Step;
            else
                Value = Maximum;
            await Task.Delay(100);
            await _rightbutton.ScaleTo(1, 50, Easing.Linear);
        }

        private async void Top_Click(object sender, EventArgs e)
        {
            AnchorX = 0.48;
            AnchorY = 0.48;
            await _topbutton.ScaleTo(0.8, 50, Easing.Linear);
            if (!IsMAXEnable && Value + FixedStep <= Maximum)
                Value += FixedStep;
            else
                Value = Maximum;
            await Task.Delay(100);
            await _rightbutton.ScaleTo(1, 50, Easing.Linear);
        }

        private async void Left_Click(object sender, EventArgs e)
        {
            AnchorX = 0.48;
            AnchorY = 0.48;
            await _leftbutton.ScaleTo(0.8, 50, Easing.Linear);
            if (Value - Step >= Minimum)
                Value -= Step;
            else
                Value = Minimum;
            await Task.Delay(100);
            await _leftbutton.ScaleTo(1, 50, Easing.Linear);
        }

        private static void ValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            StyledStepper stepper = (StyledStepper)bindable;
            stepper._centerbutton.Text = MinMaxValueObtain(stepper.Minimum, stepper.Maximum, stepper.Value).ToString();
            stepper._centerbutton.Text = (int)newValue < stepper.Minimum ? stepper.Minimum.ToString() : (int)newValue > stepper.Maximum ? stepper.Maximum.ToString() : newValue.ToString();
        }

        private static void StepperColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var stepper = (StyledStepper)bindable;
            stepper.AddStyles();
            stepper._leftbutton.Style = stepper.Resources["unSelectedStyle"] as Style;
            stepper._centerbutton.Style = stepper.Resources["selectedStyle"] as Style;
            stepper._rightbutton.Style = stepper.Resources["unSelectedStyle"] as Style;
        }

        private static void SecondaryColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var stepper = (StyledStepper)bindable;
            stepper.AddStyles();
            stepper._topbutton.Style = stepper.Resources["selectedStyle"] as Style;
        }

        private static int MinMaxValueObtain(int min, int max, int value) => value < min ? min : value > max ? max : value;
        #endregion

        void AddStyles()
        {
            var unSelectedStyle = new Style(typeof(Button))
            {
                Setters = {
                    new Setter { Property = BackgroundColorProperty,   Value = Color.Transparent },
                    new Setter { Property = Button.BorderColorProperty,   Value = StepperColor },
                    new Setter { Property = Button.TextColorProperty,   Value = StepperColor },
                    new Setter { Property = Button.BorderWidthProperty,   Value = 0.5 },
                    new Setter { Property = Button.CornerRadiusProperty,   Value = 20 },
                    new Setter { Property = HeightRequestProperty,   Value = 40 },
                    new Setter { Property = WidthRequestProperty,   Value = 40 ,
                    }
            }
            };
            var auxButtonStyle = new Style(typeof(Button))
            {
                Setters = {
                    new Setter { Property = BackgroundColorProperty, Value = SecondaryColor },
                    new Setter { Property = Button.TextColorProperty, Value = Color.White },
                    new Setter { Property = Button.BorderColorProperty, Value = SecondaryColor },
                    new Setter { Property = Button.BorderWidthProperty,   Value = 0.5 },
                    new Setter { Property = Button.CornerRadiusProperty,   Value = 20 },
                    new Setter { Property = HeightRequestProperty,   Value = 40 },
                    new Setter { Property = WidthRequestProperty,   Value = 40 },
                    new Setter { Property = Button.FontAttributesProperty,   Value = FontAttributes.Bold }
            }
            };
            var selectedStyle = new Style(typeof(Button))
            {
                Setters = {
                    new Setter { Property = BackgroundColorProperty, Value = StepperColor },
                    new Setter { Property = Button.TextColorProperty, Value = Color.White },
                    new Setter { Property = Button.BorderColorProperty, Value = StepperColor },
                    new Setter { Property = Button.BorderWidthProperty,   Value = 0.5 },
                    new Setter { Property = Button.CornerRadiusProperty,   Value = 20 },
                    new Setter { Property = HeightRequestProperty,   Value = 40 },
                    new Setter { Property = WidthRequestProperty,   Value = 40 },
                    new Setter { Property = Button.FontAttributesProperty,   Value = FontAttributes.Bold }
            }
            };
            Resources = new ResourceDictionary
            {
                {nameof(unSelectedStyle), unSelectedStyle },
                {nameof(selectedStyle), selectedStyle },
                {nameof(auxButtonStyle), auxButtonStyle },
            };
        }
    }
}
