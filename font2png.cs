using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Drawing.Text;
using System.Text;
using System.Text.RegularExpressions;

namespace font2png
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class font2png
	{
		string iniFileName = "font.ini";
		IniFile iniFile;

		// Font parameters
		int width, height;
		string fontFace;
		int fontSize;
		TextRenderingHint antiAlias;
		string fileName;
		bool alpha;
		Bitmap bmpFont;
		Color foreColor;
		Color backColor;

		bool isUnicode = false;
		int startChar = 0;
		int endChar = 256;
		int cols = 16;

		public font2png()
		{
		}

		private string ToPSPHex(Color c)
		{
			// RGB of Color are 8 bits, we want 5
			int r = c.R >> 3;
			int g = c.G >> 3;
			int b = c.B >> 3;
			
			// Color ordering is x-B-G-R 1555
			int color = r | (g << 5) | (b << 10);
			return "0x" + color.ToString("X");
		}

		private void saveFont()
		{
			//bmpFont.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
		}

		private void makeFont()
		{
            Font fnt = new Font(fontFace, fontSize);

            byte[] bytArray = new byte[8];
            bmpFont = new Bitmap(width, height);

            Graphics bmpG = Graphics.FromImage(bmpFont);
            bmpG.TextRenderingHint = antiAlias;
            Brush fntBrush = new SolidBrush(foreColor);
            if (alpha)
            {
                bmpG.Clear(Color.Transparent);
            }
            else
            {
                bmpG.Clear(backColor);
            }
            string unicodeString = "U+98DB";

            string s = ReverseToUniCode(unicodeString);
		
            //�����̗̈�T�C�Y���擾����  
            StringFormat sf = new StringFormat();
            SizeF stringSize = bmpG.MeasureString(s, fnt, bmpFont.Width, sf);

            //������`�悷��  
            //bmpG.DrawString(s, fnt, fntBrush, (bmpFont.Width / 2) - (stringSize.Width / 2), (bmpFont.Height / 2) - (stringSize.Height / 2));  
            bmpG.DrawString(s, fnt, fntBrush, 0, 0);  



            bmpFont.Save(fileName + unicodeString + ".png", System.Drawing.Imaging.ImageFormat.Png);
		}

        /// <summary>

        /// Unicode�\���������Unicode�ɕϊ����܂��B

        /// </summary>

        /// <remarks>

        /// unicode�\��(\u+XXXX)��Unicode������ɕϊ����܂��B

        /// ToUnicodeExpression�̑΂ɂȂ郁�\�b�h�ł�

        /// </remarks>

        /// <param name="str">Unicode�\��������(\uXXXX)</param>

        /// <returns>Unicode������</returns>

        public static string ReverseToUniCode(string str)
        {

            //���K�\���Ń��j�R�[�h�\��������������E���o���܂��B

            IList codeList = new ArrayList();

            Regex regUnicode = new Regex(@"(U\+){1}[0-9a-fA-F]{4,5}");

            for (Match matchUniCode = regUnicode.Match(str);

                        matchUniCode.Success;

                                matchUniCode = matchUniCode.NextMatch())
            {

                codeList.Add(matchUniCode.Groups[0].Value.Replace(@"U+", ""));

            }

            StringBuilder sb = new StringBuilder();

            //���g���G���f�B�A��������O��ɂ��Ă���̂�

            //0,1�����ڂ�2,3�����ڂ̑g��16�i������Ƃ݂Ȃ��A���l�ɕϊ����܂��B

            //0,1�����ڂ̑g�ƁA2,3�����ڂ̑g�̏��������ւ���byte�z����쐬���A

            //�G���R�[�f�B���O���s���܂��B

            //��L���K�\���Ƀ}�b�`���Ă���ȏ�Achar�z���4���邱�Ƃ��ۏ؂���Ă��܂��B

            //(�z�񐔂̃`�F�b�N�͂��܂���)

            foreach (string unicode in codeList)
            {

                char[] codeArray = unicode.ToCharArray();

                int intVal1 = Convert.ToByte(codeArray[0].ToString(), 16) *

                                     16 + Convert.ToByte(codeArray[1].ToString(), 16);

                int intVal2 = Convert.ToByte(codeArray[2].ToString(), 16) *

                                     16 + Convert.ToByte(codeArray[3].ToString(), 16);

                sb.Append(Encoding.Unicode.GetString(

                                new byte[] { (byte)intVal2, (byte)intVal1 }));

            }

            return sb.ToString();

        }

		private void assertIniFile()
		{
			iniFile.assertKeyExists("font", "width"); 
			iniFile.assertKeyExists("font", "height");
			iniFile.assertKeyExists("font", "fontName");
			iniFile.assertKeyExists("font", "fontSize");
			iniFile.assertKeyExists("font", "antiAlias");
			iniFile.assertKeyExists("font", "outputFile");
			iniFile.assertKeyExists("font", "alpha");
			iniFile.assertKeyExists("font", "backColor");
			iniFile.assertKeyExists("font", "foreColor");
		}
		
		private Color stringToColor(string str)
		{
			int iVal = int.Parse(str, System.Globalization.NumberStyles.HexNumber);
			return Color.FromArgb(iVal);
		}

		private void parseIniSettings()
		{
			width = int.Parse(iniFile.getValue("font", "width"));
			height = int.Parse(iniFile.getValue("font", "height"));
			fontSize = int.Parse(iniFile.getValue("font", "fontSize"));
			fontFace = iniFile.getValue("font", "fontName");
            fileName = iniFile.getValue("font", "outputFile");
			alpha = bool.Parse(iniFile.getValue("font", "alpha"));
            
			string smoothStr = iniFile.getValue("font", "antiAlias");
			if(smoothStr == "none")
			{
				antiAlias = TextRenderingHint.SingleBitPerPixelGridFit;
			}
			else if(smoothStr == "simple")
			{
				antiAlias = TextRenderingHint.AntiAliasGridFit;
			}
			else if(smoothStr == "subpixel")
			{
				antiAlias = TextRenderingHint.ClearTypeGridFit;
			}
			else
			{
				throw new Exception("Invalid antiAlias setting");
			}
			
			backColor = stringToColor(iniFile.getValue("font", "backColor"));
			foreColor = stringToColor(iniFile.getValue("font", "foreColor"));
			

			string unicodeStart = iniFile.getValue("unicode", "startChar");
			if(unicodeStart != null)
			{
				startChar = int.Parse(unicodeStart, System.Globalization.NumberStyles.HexNumber);
				isUnicode = true;
			}

			iniFile.assertKeyExists("unicode", "endChar");
			endChar = int.Parse(iniFile.getValue("unicode", "endChar"), System.Globalization.NumberStyles.HexNumber);
			
			string colsStr = iniFile.getValue("unicode", "cols");

			if(colsStr != null)
			{
				cols = int.Parse(colsStr);
			}

		}

		private void displayUsage()
		{
			System.Console.Out.WriteLine("font2png [font]");
			System.Console.Out.WriteLine(" font is the name of the ini file used to make the font.");
			System.Console.Out.WriteLine(" If font is not specified, font.ini will be used.");
		}

		private void processCommandLine(string [] args)
		{
			if(args.Length == 0)
			{
				iniFile = new IniFile("./font.ini");
			}
			else if(args.Length == 1)
			{
				if(args[0] == "-?")
				{
					displayUsage();
					return;
				}
				iniFile = new IniFile(iniFileName);
			}
			else
			{
				displayUsage();
				return;
			}
		}

		private void run(string[] args)
		{
			processCommandLine(args);
			assertIniFile();
			parseIniSettings();	
			makeFont();
			saveFont();
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) 
		{
			try
			{
				new font2png().run(args);
			}
			catch(Exception e)
			{
				System.Console.Out.WriteLine("Error: " + e.Message);
				return;
			}
			System.Console.Out.WriteLine("Font created successfully");
		}


	}
}

