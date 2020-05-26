using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using System.Text;

using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;

namespace Semmelweis_Drive
{
    public class Program
    {

        /* create PWMs on pwm_4, 7, 8 */
        static uint period = 25000; //period between pulses
        static uint duration = 2300; //default duration of pulse
        static PWM left_pwm = new PWM(CTRE.HERO.IO.Port3.PWM_Pin4, period, duration, PWM.ScaleFactor.Microseconds, false);
        static PWM right_pwm = new PWM(CTRE.HERO.IO.Port3.PWM_Pin7, period, duration, PWM.ScaleFactor.Microseconds, false);
        static PWM servo_pwm = new PWM(CTRE.HERO.IO.Port3.PWM_Pin8, period, duration, PWM.ScaleFactor.Microseconds, false);

        /* create output strings */
        static StringBuilder stringBuilder = new StringBuilder();

        static CTRE.Phoenix.Controller.GameController _gamepad = new GameController(UsbHostDevice.GetInstance());

        static uint servo_setting = 2300;
        static uint left_pwm_setting = 1500;
        static uint right_pwm_setting = 1500;

        public static void Main()
        {
            /* Start the PWMs */
            left_pwm.Start();
            right_pwm.Start();
            servo_pwm.Start();

            /* loop forever */
            while (true)
            {
                /* drive robot using gamepad */
                Drive();
                /* print whatever is in our string builder */
                Debug.Print(stringBuilder.ToString());
                stringBuilder.Clear();
                /* feed watchdog to keep outputs enabled */
                CTRE.Phoenix.Watchdog.Feed();
                /* run this task every 20ms */
                Thread.Sleep(20);
            }
        }
        /**
         * If value is within 10% of center, clear it.
         * @param value [out] floating point value to deadband.
         */
        static void Deadband(ref float value)
        {
            if (value < -0.03)
            {
                /* outside of deadband */
            }
            else if (value > +0.03)
            {
                /* outside of deadband */
            }
            else
            {
                /* within band so zero it */
                value = 0;
            }
        }
        static void drive_mix(float leftThrot, float rightThrot)
        {
            /* Full speed forward */
            uint forward = 2500;
            /* Full speed backward */
            uint backward = 500;

            left_pwm_setting = (uint)((forward - backward) / 2 * leftThrot) + 1500;
            right_pwm_setting = (uint)((forward - backward) / 2 * rightThrot) + 1500;

            if (left_pwm_setting < backward)
            {
                left_pwm_setting = backward;
            }
            if (right_pwm_setting < backward)
            {
                right_pwm_setting = backward;
            }
            if (left_pwm_setting > forward)
            {
                left_pwm_setting = forward;
            }
            if (right_pwm_setting > forward)
            {
                right_pwm_setting = forward;
            }

        }
        static void Drive()
        {
            float y = (float)-0.5 * _gamepad.GetAxis(1);
            float twist = (float)0.17 * _gamepad.GetAxis(2);

            bool button1 = _gamepad.GetButton(1);
            bool button3 = _gamepad.GetButton(3);
            bool lt = _gamepad.GetButton(7);
            bool rt = _gamepad.GetButton(8);

            Deadband(ref y);
            Deadband(ref twist);

            // Cubic transform for forward/back
            y = y * y * y;

            // Overdrive if both throttles are set
            if (lt && rt && y > 0.0)
            {
                y = (float)2.6 * y;
                twist = (float)0.6 * twist;
            }

            float leftThrot = -(y + twist);
            float rightThrot = -(y - twist);

            if (button1)
            {
                servo_setting = 2300;
            }
            if (button3)
            {
                servo_setting = 900;   //1250
            }

            drive_mix(leftThrot, rightThrot);

            servo_pwm.Duration = servo_setting;
            left_pwm.Duration = left_pwm_setting;
            right_pwm.Duration = right_pwm_setting;

            stringBuilder.Append(y);
            stringBuilder.Append("\t");
            stringBuilder.Append(twist);
            stringBuilder.Append("\t");
            stringBuilder.Append(left_pwm_setting);
            stringBuilder.Append("\t");
            stringBuilder.Append(right_pwm_setting);
            stringBuilder.Append("\t");
            stringBuilder.Append(button1);
            stringBuilder.Append("\t");
            stringBuilder.Append(button3);
            stringBuilder.Append("\t");
            stringBuilder.Append(servo_setting);
            stringBuilder.Append("\t");
            stringBuilder.Append(lt);
            stringBuilder.Append("\t");
            stringBuilder.Append(rt);

        }
    }
}
