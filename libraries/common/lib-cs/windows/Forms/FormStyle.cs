/**
 *
 * \ingroup Windows
 *
 * \copyright
 *   Copyright (c) 2008-2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D et al. / SpringCard
 *
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using System.Drawing;
using System.Windows.Forms;

namespace SpringCard.LibCs.Windows.Forms
{
	public enum FormStyle
	{
        Default = -1,
		Classical = 0,
        Modern,
		ModernRed,
        ModernMarroon
	}

    public enum ControlType
    {
        Unknown,
        Basic,
        Fixed,
        Link,
        Heading
    }

    public static class Forms
    {
        public static FormStyle DefaultStyle = FormStyle.Classical;

        private static FontFamily FixedFontFamily = new FontFamily("Consolas");

        private static Font ModernDefaultFont = new Font("Calibri Light", 10, FontStyle.Regular);
        private static Font ModernHeadingFont = new Font("Calibri Light", 24, FontStyle.Regular);

        public static Color BlackColor = Color.FromArgb(0, 0, 0);
        public static Color WhiteColor = Color.FromArgb(255, 255, 255);

        public static Color ClassicColor = Color.FromArgb(240, 240, 240);
        public static Color DarkClassicColor = Color.FromArgb(218, 218, 218);
        public static Color RedColor = Color.FromArgb(216, 10, 29);
        public static Color MarroonColor = Color.FromArgb(174, 141, 128);
        public static Color DarkRedColor = Color.FromArgb(153, 7, 20);
        public static Color DarkMarroonColor = Color.FromArgb(123, 100, 91);

        public static ControlType GuessControlType(Control control)
        {
            ControlType result = ControlType.Unknown;

            if (control is LinkLabel)
            {
                result = ControlType.Link;
            }
            else if (control.Font.FontFamily.Equals(FixedFontFamily))
            {
                result = ControlType.Fixed;
            }
            else
            { 
                if (control.Font.Size <= 12)
                {
                    result = ControlType.Basic;
                }
                else if (control.Font.Size >= 14)
                {
                    result = ControlType.Heading;
                }
            }

            return result;
        }

        public static Font GetTextFont(FormStyle style, ControlType controlType)
        {
            Font result = null;

            if (style >= FormStyle.Modern)
            {
                switch (controlType)
                {
                    case ControlType.Basic:
                    case ControlType.Link:
                        result = ModernDefaultFont;
                        break;
                    case ControlType.Heading:
                        result = ModernHeadingFont;
                        break;
                }
            }

            return result;
        }

        public static Color GetTextColor(FormStyle style, ControlType controlType)
        {
            Color result = Color.Transparent;

            if (style >= FormStyle.Modern)
            {
                switch (controlType)
                {
                    case ControlType.Basic:
                        result = BlackColor;
                        break;

                    case ControlType.Heading:
                        switch (style)
                        {
                            case FormStyle.Modern:
                                result = RedColor;
                                break;

                            case FormStyle.ModernRed:
                            case FormStyle.ModernMarroon:
                                result = WhiteColor;
                                break;
                        }
                        break;

                    case ControlType.Link:
                        result = RedColor;
                        break;
                }
            }

            return result;
        }

        public static Color GetHeaderColor(FormStyle style)
        {
            Color result = Color.Transparent;

            switch (style)
            {
                case FormStyle.ModernRed:
                    result = RedColor;
                    break;

                case FormStyle.ModernMarroon:
                    result = MarroonColor;
                    break;
            }

            return result;
        }

        public static void ApplyStyle(Control.ControlCollection controls, FormStyle style)
        {
            foreach (Control control in controls)
            {
                if (control is Panel)
                {
                    if (control.Name.ToLower().Contains("header"))
                    {
                        Color backColor = GetHeaderColor(style);
                        if (backColor != Color.Transparent)
                            control.BackColor = backColor;
                    }
                }

                if (control is PictureBox)
                {
                    if (control.Name.ToLower().Contains("logowhite"))
                    {
                        switch (style)
                        {
                            case FormStyle.ModernRed:
                            case FormStyle.ModernMarroon:
                                control.Visible = true;
                                break;
                            default:
                                control.Visible = false;
                                break;
                        }
                    }

                    if (control.Name.ToLower().Contains("logocolor"))
                    {
                        switch (style)
                        {
                            case FormStyle.ModernRed:
                            case FormStyle.ModernMarroon:
                                control.Visible = false;
                                break;
                            default:
                                control.Visible = true;
                                break;
                        }
                    }
                }

                ControlType type = GuessControlType(control);

                Font font = GetTextFont(style, type);
                Color foreColor = GetTextColor(style, type);

                if (font != null)
                {
                    control.Font = font;
                    /* Correct Y position */
                    if ((type == ControlType.Heading) && (style >= FormStyle.Modern))
                        control.Top -= 6;
                }

                if (foreColor != Color.Transparent)
                {
                    control.ForeColor = foreColor;
                    /* Link */
                    if (control is LinkLabel)
                    {
                        LinkLabel link = (LinkLabel)control;
                        link.ActiveLinkColor = link.ForeColor;
                        link.LinkColor = link.ForeColor;
                        link.VisitedLinkColor = link.ForeColor;
                    }
                }

                if (control.HasChildren)
                    ApplyStyle(control.Controls, style);
            }
        }

        public static void ApplyStyle(Form form, FormStyle style)
        {
            form.Font = GetTextFont(style, ControlType.Basic);
            ApplyStyle(form.Controls, style);
        }
    }
}
