using Newtonsoft.Json;
using SharpDX.XInput;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ControllerView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Controller controller = null;

        public MainWindow()
        {
            InitializeComponent();

            var json = "";
            using (var fs = File.OpenRead("Config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();
            var cfg = JsonConvert.DeserializeObject<ConfigLoader>(json);
            var converter = new BrushConverter();

            int deadzone = 3500;
            float AccelDecelRange = 545f;
            if (cfg.GearDown.ButtonNumber == 0 || cfg.GearUp.ButtonNumber == 0)
            {
                AccelDecelRange = 585f;
                this.GearUp.Visibility = Visibility.Hidden;
                this.GearDown.Visibility = Visibility.Hidden;
            }

            Style style = new Style(typeof(Label));
            style.Setters.Add(new Setter(Label.BorderBrushProperty, Brushes.Black));
            style.Setters.Add(new Setter(Label.BorderThicknessProperty, new Thickness(cfg.Colours.HaveBorders ? 1 : 0)));
            HandBrakeLabel.Style = style;
            NosLabel.Style = style;
            BoTLabel.Style = style;
            Accelerate.Style = style;
            Decelerate.Style = style;
            AccLabel.Style = style;
            DecLabel.Style = style;
            this.LeftLabel.Style = style;
            this.RightLabel.Style = style;
            this.GearDown.Style = style;
            this.GearUp.Style = style;

            this.Accelerate.Width = AccelDecelRange;
            this.AccLabel.Width = AccelDecelRange;
            this.Decelerate.Width = AccelDecelRange;
            this.DecLabel.Width = AccelDecelRange;

            this.Accelerate.Background = (Brush)converter.ConvertFromString(cfg.Colours.AccelHex);
            this.Decelerate.Background = (Brush)converter.ConvertFromString(cfg.Colours.DecelHex);
            this.LeftTurn.Fill = (Brush)converter.ConvertFromString(cfg.Colours.LeftHex);
            this.RightTurn.Fill = (Brush)converter.ConvertFromString(cfg.Colours.RightHex);
            var controllers = new[] { new Controller(UserIndex.One), new Controller(UserIndex.Two), new Controller(UserIndex.Three), new Controller(UserIndex.Four) };

            controller = FindController(controllers);

            ControllerButton NitrosButton = new ControllerButton()
            {
                Button = (GamepadButtonFlags)cfg.Nitro.ButtonNumber,
                Colour = (Brush)converter.ConvertFromString(cfg.Nitro.ColourHex),
                Label = this.NosLabel
            };
            ControllerButton HandBrakeButton = new ControllerButton()
            {
                Button = (GamepadButtonFlags)cfg.Handbrake.ButtonNumber,
                Colour = (Brush)converter.ConvertFromString(cfg.Handbrake.ColourHex),
                Label = this.HandBrakeLabel
            };
            ControllerButton GearUp = new ControllerButton()
            {
                Button = (GamepadButtonFlags)cfg.GearUp.ButtonNumber,
                Colour = (Brush)converter.ConvertFromString(cfg.GearUp.ColourHex),
                Label = this.GearUp
            };
            ControllerButton GearDown = new ControllerButton()
            {
                Button = (GamepadButtonFlags)cfg.GearDown.ButtonNumber,
                Colour = (Brush)converter.ConvertFromString(cfg.GearDown.ColourHex),
                Label = this.GearDown
            };
            ControllerButton BoT = new ControllerButton()
            {
                Button = (GamepadButtonFlags)cfg.BoT.ButtonNumber + cfg.BoT.ComboButtonNumber,
                Colour = (Brush)converter.ConvertFromString(cfg.BoT.ColourHex),
                Label = this.BoTLabel
            };

            if (controller == null)
            {
                new Thread(() =>
                {
                    while (controller == null)
                    {
                        controller = FindController(controllers);
                    }
                }).Start();
            }

            new Thread(() =>
            {
                while (true)
                {
                    if (controller != null)
                    {
                        if (!controller.IsConnected)
                        {
                            controller = FindController(controllers);
                        }
                        while (controller.IsConnected)
                        {
                            var previousState = controller.GetState();
                            var state = controller.GetState();

                            ButtonPress(previousState, NitrosButton);
                            ButtonPress(previousState, HandBrakeButton);
                            ButtonPress(previousState, GearUp);
                            ButtonPress(previousState, GearDown);
                            ButtonPress(previousState, BoT);

                            Accelerate.Dispatcher.Invoke(new Action(() => { Accelerate.Width = AccelDecelRange - (AccelDecelRange / 255 * (255 - previousState.Gamepad.RightTrigger)); }));
                            Decelerate.Dispatcher.Invoke(new Action(() => { Decelerate.Width = AccelDecelRange - (AccelDecelRange / 255 * (255 - previousState.Gamepad.LeftTrigger)); }));

                            if (previousState.Gamepad.LeftThumbX < 0 && Math.Abs((float)previousState.Gamepad.LeftThumbX) > deadzone)
                            {
                                RightTurn.Dispatcher.Invoke(new Action(() => { RightTurn.Width = 0; }));
                                LeftTurn.Dispatcher.Invoke(new Action(() => { LeftTurn.Width = 397f - (397f / (32768 - deadzone) * ((32768 - deadzone) + (previousState.Gamepad.LeftThumbX + deadzone))); }));
                            }
                            else if (previousState.Gamepad.LeftThumbX > 0 && Math.Abs((float)previousState.Gamepad.LeftThumbX) > deadzone)
                            {
                                LeftTurn.Dispatcher.Invoke(new Action(() => { LeftTurn.Width = 0; }));
                                RightTurn.Dispatcher.Invoke(new Action(() => { RightTurn.Width = 397f - (397f / (32768 - deadzone) * ((32768 - deadzone) - (previousState.Gamepad.LeftThumbX - deadzone))); }));
                            }
                            else
                            {
                                LeftTurn.Dispatcher.Invoke(new Action(() => { LeftTurn.Width = 2; }));
                                RightTurn.Dispatcher.Invoke(new Action(() => { RightTurn.Width = 2; }));
                            }
                            Thread.Sleep(10);
                            previousState = state;
                        }
                    }
                }
            }).Start();
        }

        private void ButtonPress(State previousState, ControllerButton Button)
        {
            if (previousState.Gamepad.Buttons.ToString().Contains(Button.Button.ToString()))
            {
                Button.Label.Dispatcher.Invoke(new Action(() => { Button.Label.Background = Button.Colour; }));
            }
            else
            {
                Button.Label.Dispatcher.Invoke(new Action(() => { Button.Label.Background = Brushes.Transparent; }));
            }
        }

        private Controller FindController(Controller[] controllers)
        {
            foreach (var selectControler in controllers)
            {
                if (selectControler.IsConnected)
                {
                    return selectControler;
                }
            }
            return controller;
        }

        internal class ControllerButton
        {
            public Label Label { get; set; }
            public Brush Colour { get; set; }
            public GamepadButtonFlags Button { get; set; }
            public GamepadButtonFlags Button2 { get; set; }
        }
    }
}