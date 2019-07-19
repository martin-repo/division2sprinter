// -------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Martin Karlsson">
//   Copyright (c) 2018 Martin Karlsson. All rights reserved.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace Division2Sprinter
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Division2Sprinter.Properties;

    using JetBrains.Annotations;

    using SharpDX.XInput;

    internal class Program
    {
        /// <summary>
        /// Customize the behavior of Division2Sprinter by specifying input arguments.
        /// Start without arguments for default behavior and usage instructions.
        /// </summary>
        /// <param name="controllerIndex">Controller to use for input (Default=One).</param>
        /// <param name="mouseButton">
        /// Mouse button to hold when left thumbstick exceeds the threshold (Default=Left).
        /// XButton1=MB4, XButton2=MB5.
        /// </param>
        /// <param name="thresholdPercent">
        /// Threshold in percent, 1-100 (Default=70).
        /// Less than 20 is not recommended due to thumbstick dead zone.
        /// </param>
        [UsedImplicitly]
        private static void Main(
            UserIndex controllerIndex = UserIndex.One,
            SimulatedMouseButton mouseButton = SimulatedMouseButton.Left,
            int thresholdPercent = 70)
        {
            var settings = Settings.Default;

            if (thresholdPercent < 1 || thresholdPercent > 100)
            {
                Console.WriteLine(settings.ThresholdError);
                Console.ReadKey();
                return;
            }

            Console.WriteLine(settings.Instructions);
            Console.WriteLine();

            var controller = new Controller(controllerIndex);

            var isConnected = controller.IsConnected;
            if (!isConnected)
            {
                Console.WriteLine(settings.ControllerError);
                Console.ReadKey();
                return;
            }

            Console.WriteLine(settings.Started);
            Console.WriteLine();

            var cancellationTokenSource = new CancellationTokenSource();
            Task.Run(
                () =>
                    {
                        Console.ReadKey();
                        cancellationTokenSource.Cancel();
                    });

            var controllerMod = new ControllerMod();

            // Start always sprint modification
            controllerMod.AlwaysSprintAsync(controller, mouseButton, thresholdPercent, cancellationTokenSource.Token).Wait();
        }
    }
}