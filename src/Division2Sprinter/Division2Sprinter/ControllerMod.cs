// -------------------------------------------------------------------------------------------------
// <copyright file="ControllerMod.cs" company="Martin Karlsson">
//   Copyright (c) 2018 Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace Division2Sprinter
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;

    using Division2Sprinter.Properties;

    using SharpDX.XInput;

    using WindowsInput;

    public class ControllerMod
    {
        // Left Mouse Button = Button 1
        // Right Mouse Button = Button 2
        // Middle Mouse Button = Button 3
        // XButton1 = Button 4
        // XButton2 = Button 5
        private const short XButton1 = 0x0001;
        private const short XButton2 = 0x0002;

        private static readonly TimeSpan ControllerPollInterval = TimeSpan.FromMilliseconds(10);

        public async Task AlwaysSprintAsync(
            Controller controller,
            SimulatedMouseButton mouseButton,
            int thresholdPercent,
            CancellationToken cancellationToken)
        {
            try
            {
                var inputSimulator = new InputSimulator();

                var sprintIndex = 0;
                var totalSprintDuration = new TimeSpan();
                var sprintStarted = DateTime.Now;

                var settings = Settings.Default;

                var sprintActive = false;
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    // Get current controller state
                    var state = controller.GetState();
                    var gamepad = state.Gamepad;

                    // Get thumbstick radius from the center
                    var radius = GetRadius(gamepad.LeftThumbX, gamepad.LeftThumbY);

                    // Sprint when thumbstick radius is more than 70% of max 
                    // Radius in X or Y direction is 32767, but in 45 degree angle it's over 37000...(?)
                    // The difference is not that big, to calculate percentage value from "normal" max value
                    var shouldSprint = radius >= thresholdPercent / 100f * short.MaxValue;
                    if (shouldSprint && !sprintActive)
                    {
                        sprintActive = true;
                        sprintStarted = DateTime.Now;

                        var sprintIndexString = (++sprintIndex).ToString("N0", CultureInfo.InvariantCulture);
                        Console.Write($"{settings.Sprint} #{sprintIndexString} {DateTime.Now:HH:mm:ss} >> ");

                        // Simulate mouse button down
                        MouseButtonDown(inputSimulator, mouseButton);
                    }
                    else if (!shouldSprint && sprintActive)
                    {
                        sprintActive = false;
                        var sprintDuration = DateTime.Now - sprintStarted;
                        totalSprintDuration = totalSprintDuration.Add(sprintDuration);
                        var averageDuration = totalSprintDuration.TotalSeconds / sprintIndex;

                        Console.WriteLine(
                            $"{DateTime.Now:HH:mm:ss} {settings.Duration}={sprintDuration.TotalSeconds:F2}s ({settings.AverageDuration}={averageDuration:F2}s)");

                        // Simulate mouse button up
                        MouseButtonUp(inputSimulator, mouseButton);
                    }

                    // Wait before getting controller state again
                    await Task.Delay(ControllerPollInterval, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        private static int GetRadius(int x, int y)
        {
            // Get radius from a center point that is x=0, y=0
            var absoluteX = Math.Abs(x);
            var absoluteY = Math.Abs(y);
            var radius = Math.Sqrt(Math.Pow(absoluteX, 2) + Math.Pow(absoluteY, 2));
            return (int)radius;
        }

        private static void MouseButtonDown(InputSimulator inputSimulator, SimulatedMouseButton mouseButton)
        {
            switch (mouseButton)
            {
                case SimulatedMouseButton.Left:
                    inputSimulator.Mouse.LeftButtonDown();
                    break;
                case SimulatedMouseButton.Right:
                    inputSimulator.Mouse.RightButtonDown();
                    break;
                case SimulatedMouseButton.XButton1:
                    inputSimulator.Mouse.XButtonDown(XButton1);
                    break;
                case SimulatedMouseButton.XButton2:
                    inputSimulator.Mouse.XButtonDown(XButton2);
                    break;
                default:
                    Console.WriteLine($"Unrecognized mouse button {mouseButton}");
                    break;
            }
        }

        private static void MouseButtonUp(InputSimulator inputSimulator, SimulatedMouseButton mouseButton)
        {
            switch (mouseButton)
            {
                case SimulatedMouseButton.Left:
                    inputSimulator.Mouse.LeftButtonUp();
                    break;
                case SimulatedMouseButton.Right:
                    inputSimulator.Mouse.RightButtonUp();
                    break;
                case SimulatedMouseButton.XButton1:
                    inputSimulator.Mouse.XButtonUp(XButton1);
                    break;
                case SimulatedMouseButton.XButton2:
                    inputSimulator.Mouse.XButtonUp(XButton2);
                    break;
                default:
                    Console.WriteLine($"Unrecognized mouse button {mouseButton}");
                    break;
            }
        }
    }
}