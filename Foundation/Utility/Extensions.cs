// name=Extensions.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotNetNuke.Modules.Foundation
{
    /// <summary>
    /// Collection of extension methods (based on submitted large helper file).
    /// - Target: .NET Framework 4.7.2
    /// - All original method names/signatures preserved; many internals fixed/strengthened.
    /// </summary>
    public static class Extensions
    {
        #region Regex caches & helpers
        private static readonly Regex _digitsRegex = new Regex(@"^\d+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _intListSplitRegex = new Regex(@"[,\;\|\x22\x27\r\n\t\s\(\)\[\]\{\}]+", RegexOptions.Compiled);
        private static readonly Regex _tokenRegex = new Regex(@"^[a-zA-Z0-9\-]+$", RegexOptions.Compiled);
        private static readonly Regex _timeRegex = new Regex(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9](:[0-5][0-9])?$", RegexOptions.Compiled);
        private static readonly Regex _removeNewLineRegex = new Regex(@"(\r\n?|\n)", RegexOptions.Compiled);
        private static readonly Regex _multipleWhiteSpaceRegex = new Regex(@"\s+", RegexOptions.Compiled);
        #endregion

        #region Numeric and number-detection

        /// <summary>
        /// Original IsNumerical but fixed & extended to handle examples in header:
        /// - supports hex (0x...), binary (0b...), octal (leading 0 with digits 0-7), decimal (incl. exponent).
        /// - +0999 will be treated as decimal (forced by leading sign).
        /// - 0999 will be false (contains 9 - not octal).
        /// </summary>
        public static bool IsNumerical(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;
            str = str.Trim();

            // Binary 0b...
            if (str.Length > 2 && (str.StartsWith("0b", StringComparison.OrdinalIgnoreCase)))
            {
                var d = str.Substring(2);
                return d.Length > 0 && d.All(c => c == '0' || c == '1');
            }

            // Hex 0x...
            if (str.Length > 2 && (str.StartsWith("0x", StringComparison.OrdinalIgnoreCase)))
            {
                var d = str.Substring(2);
                return d.Length > 0 && d.All(c => Uri.IsHexDigit(c));
            }

            // If explicit sign, treat as decimal/floating (force decimal)
            if (str.StartsWith("+") || str.StartsWith("-"))
            {
                return IsDouble(str);
            }

            // Octal: leading 0 and all digits 0-7 (and not hex/binary)
            if (str.Length > 1 && str[0] == '0' && str.All(c => char.IsDigit(c)))
            {
                // if any digit 8 or 9 then not octal (like 0999)
                if (str.Skip(1).Any(c => c == '8' || c == '9'))
                    return false;
                // digits only 0-7 -> octal
                return str.Skip(1).All(c => c >= '0' && c <= '7');
            }

            // Decimal/float/exponent using TryParse (invariant culture preferred)
            return IsDouble(str);
        }

        /// <summary>
        /// Only digits (0-9)
        /// </summary>
        public static bool IsNumeric(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;
            return _digitsRegex.IsMatch(str.Trim());
        }

        public static bool IsInt32(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;
            int result;
            return int.TryParse(str.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        public static bool IsInt64(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;
            long result;
            return long.TryParse(str.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        public static bool IsDouble(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;
            double result;
            // Accept exponent and decimal point using invariant culture first
            if (double.TryParse(str.Trim(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result))
                return true;
            // Fallback to current culture parse
            if (double.TryParse(str.Trim(), NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out result))
                return true;
            return false;
        }

        #endregion

        #region Boolean / Token / Simple checks

        public static bool IsBool(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;
            var s = str.Trim().ToLowerInvariant();
            return s == "true" || s == "false" || s == "1" || s == "0";
        }

        public static bool IsToken(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;
            return _tokenRegex.IsMatch(str.Trim());
        }

        public static bool IsImage(this string extension)
        {
            if (string.IsNullOrWhiteSpace(extension)) return false;
            string e = extension.Trim().ToLowerInvariant();
            // accept both file names and extensions
            if (!e.StartsWith(".")) e = Path.GetExtension(e) ?? e;
            return e.Contains("jpg") || e.Contains("jpeg") || e.Contains("png") || e.Contains("gif") || e.Contains("webp") || e.Contains("bmp");
        }

        public static bool IsAnimatedGIF(this System.Drawing.Image image)
        {
            if (image == null) return false;
            return System.Drawing.ImageAnimator.CanAnimate(image);
        }

        #endregion

        #region Image transparency (original code preserved with fixes)

        public static bool IsTransparency(this System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null) return false;

            try
            {
                // not an alpha-capable color format.
                if ((bitmap.Flags & (int)System.Drawing.Imaging.ImageFlags.HasAlpha) == 0)
                    return false;

                // Indexed formats. Special case because one index on their palette is configured as THE transparent color.
                if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed || bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format4bppIndexed)
                {
                    System.Drawing.Imaging.ColorPalette pal = bitmap.Palette;
                    int transCol = -1;
                    for (int i = 0; i < pal.Entries.Length; i++)
                    {
                        System.Drawing.Color col = pal.Entries[i];
                        if (col.A != 255)
                        {
                            transCol = i;
                            break;
                        }
                    }
                    if (transCol == -1)
                        return false;

                    Int32 colDepth = System.Drawing.Image.GetPixelFormatSize(bitmap.PixelFormat);
                    System.Drawing.Imaging.BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
                    Int32 stride = Math.Abs(data.Stride);
                    Byte[] bytes = new Byte[bitmap.Height * stride];
                    System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                    bitmap.UnlockBits(data);

                    if (colDepth == 8)
                    {
                        Int32 lineMax = bitmap.Width - 1;
                        for (Int32 i = 0; i < bytes.Length; i++)
                        {
                            Int32 linepos = i % stride;
                            if (linepos > lineMax) continue;
                            Byte b = bytes[i];
                            if (b == transCol) return true;
                        }
                    }
                    else if (colDepth == 4)
                    {
                        Int32 lineMax = (bitmap.Width / 2);
                        bool halfByte = bitmap.Width % 2 != 0;
                        if (!halfByte) lineMax--;
                        for (Int32 i = 0; i < bytes.Length; i++)
                        {
                            Int32 linepos = i % stride;
                            if (linepos > lineMax) continue;
                            Byte b = bytes[i];
                            if ((b & 0x0F) == transCol) return true;
                            if (halfByte && linepos == lineMax) continue;
                            if (((b & 0xF0) >> 4) == transCol) return true;
                        }
                    }
                    return false;
                }

                if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb || bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
                {
                    System.Drawing.Imaging.BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
                    Byte[] bytes = new Byte[bitmap.Height * Math.Abs(data.Stride)];
                    System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                    bitmap.UnlockBits(data);
                    for (Int32 p = 3; p < bytes.Length; p += 4)
                    {
                        if (bytes[p] != 255) return true;
                    }
                    return false;
                }

                // fallback: slow pixel scan
                for (Int32 i = 0; i < bitmap.Width; i++)
                {
                    for (Int32 j = 0; j < bitmap.Height; j++)
                    {
                        if (bitmap.GetPixel(i, j).A != 255) return true;
                    }
                }
            }
            catch
            {
                // On error, assume no transparency
            }
            return false;
        }

        #endregion

        #region URL / Site / Email

        public static bool IsUrl(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;
            return Uri.IsWellFormedUriString(str.Trim(), UriKind.Absolute);
        }

        public static bool IsSiteUrl(this string str, Uri siteUri)
        {
            if (string.IsNullOrWhiteSpace(str) || siteUri == null) return false;
            try
            {
                Uri uri = new Uri(str);
                return string.Equals(uri.Host, siteUri.Host, StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        public static bool IsEmail(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;
            return (new System.ComponentModel.DataAnnotations.EmailAddressAttribute()).IsValid(str.Trim());
        }

        #endregion

        #region Object checks / Property helpers

        // Original IsObject(this object source, Type targetType) had inverted logic; fixed:
        public static bool IsObject(this object source, Type targetType)
        {
            if (source == null || targetType == null) return false;
            return source.GetType() == targetType;
        }

        public static bool IsObject(this System.ComponentModel.PropertyDescriptor source)
        {
            if (source == null) return false;
            var cp = source.GetChildProperties();
            return (cp != null && cp.Count > 0);
        }

        public static bool HasProperty(this Type obj, string propertyName)
        {
            if (obj == null || string.IsNullOrWhiteSpace(propertyName)) return false;
            return obj.GetProperty(propertyName) != null;
        }

        public static bool HasMethod(this object obj, string methodName)
        {
            if (obj == null || string.IsNullOrWhiteSpace(methodName)) return false;
            var type = obj.GetType();
            return type.GetMethod(methodName) != null;
        }

        public static object GetPropertyObject(this object info, string PropertyName)
        {
            if (info == null || string.IsNullOrWhiteSpace(PropertyName)) return null;
            var myType = info.GetType();
            var props = new List<PropertyInfo>(myType.GetProperties());
            var prop = props.FirstOrDefault(p => p.Name.Equals(PropertyName, StringComparison.OrdinalIgnoreCase));
            if (prop != null) return prop.GetValue(info, null);
            return null;
        }

        public static object GetPropertyValue(this object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrWhiteSpace(propertyName)) return null;
            var property = obj.GetType().GetProperty(propertyName);
            return property != null ? property.GetValue(obj) : null;
        }

        public static void SetPropertyValue(this object obj, string propertyName, object value)
        {
            if (obj == null || string.IsNullOrWhiteSpace(propertyName)) return;
            var property = obj.GetType().GetProperty(propertyName);
            if (property == null || !property.CanWrite) return;
            try
            {
                var convertedValue = value != null ? Convert.ChangeType(value, property.PropertyType) : null;
                property.SetValue(obj, convertedValue);
            }
            catch { /* ignore conversion errors */ }
        }

        #endregion

        #region Base64 / Unicode / File helpers

        public static bool IsBase64(this string base64String)
        {
            if (string.IsNullOrEmpty(base64String)) return false;
            string s = base64String.Trim();
            if (s.Length % 4 != 0) return false;
            if (s.Any(ch => char.IsWhiteSpace(ch))) return false;
            try
            {
                Convert.FromBase64String(s);
                return true;
            }
            catch { return false; }
        }

        public static bool isUnicode(this string str)
        {
            if (str == null) return false;
            var ascii = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(str));
            return !string.Equals(ascii, str, StringComparison.Ordinal);
        }

        public static bool IsFileReady(this string sFilename)
        {
            if (string.IsNullOrWhiteSpace(sFilename)) return false;
            try
            {
                using (FileStream inputStream = File.Open(sFilename, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return inputStream.Length > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Compare / Contains / Split / UrlCleaning

        public static bool Compare<T>(this string op, T left, T right) where T : IComparable<T>
        {
            if (op == null) throw new ArgumentNullException(nameof(op));
            switch (op)
            {
                case "<": return left.CompareTo(right) < 0;
                case ">": return left.CompareTo(right) > 0;
                case "<=": return left.CompareTo(right) <= 0;
                case ">=": return left.CompareTo(right) >= 0;
                case "==": return left.Equals(right);
                case "=": return left.Equals(right);
                case "!=": return !left.Equals(right);
                case "<>": return !left.Equals(right);
                default: throw new ArgumentException(string.Format("Invalid comparison operator: {0}", op));
            }
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            if (string.IsNullOrEmpty(toCheck) || string.IsNullOrEmpty(source)) return false;
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static string[] Split(this string str, string splitter)
        {
            if (str == null) return new string[0];
            if (splitter == null) return new string[] { str };
            return str.Replace(splitter, "ö").Split('ö');
        }

        public static string UrlClearing(this string url)
        {
            if (string.IsNullOrEmpty(url)) return url;
            List<string> urlList = new List<string>();
            foreach (string urlToken in url.Split('/'))
                if ((urlList.Count < 1) || (urlList.LastOrDefault().Trim().ToLower() != urlToken.Trim().ToLower()))
                    urlList.Add(urlToken);
            return String.Join("/", urlList.ToArray());
        }

        #endregion

        #region String transforms / sanitizers / safe formatting

        public static string FirstCharToUpper(this string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            var stringArray = input.ToCharArray();
            if (char.IsLower(stringArray[0])) stringArray[0] = char.ToUpper(stringArray[0]);
            return new string(stringArray);
        }

        public static string CDV(this string path)
        {
            try
            {
                string MapPath = path.MapPath();
                if (!string.IsNullOrEmpty(MapPath))
                    return System.IO.File.GetLastWriteTime(MapPath).Ticks.ToString();
            }
            catch { }
            return DateTime.Now.Ticks.ToString();
        }

        public static string ReplaceToken(this string str, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(str)) return str;
            Regex pattern = new Regex(@"[\[\{\]\}]", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
            oldValue = pattern.Replace(oldValue ?? "", "");
            oldValue = oldValue.Trim().ToLower();
            newValue = (newValue ?? "").Trim().ToLower();

            string result = str.ToLower().Replace("[" + oldValue + "]", newValue);
            result = result.ToLower().Replace("{{" + oldValue + "}}", newValue);
            result = result.ToLower().Replace("{" + oldValue + "}", newValue);

            return result;
        }

        /// <summary>
        /// ContainsToken original behavior: matches {token} or [token] or {{token}}
        /// </summary>
        public static bool ContainsToken(this string str, string token)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(token)) return false;
            string pattern = @"\{" + Regex.Escape(token.Trim()) + @"\}|\[" + Regex.Escape(token.Trim()) + @"\]";
            Match match = Regex.Match(str, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
            return match.Success;
        }

        public static string HtmlEncode(this string str)
        {
            return HttpUtility.HtmlEncode(str);
        }
        public static string HtmlDecode(this string str)
        {
            return HttpUtility.HtmlDecode(str);
        }

        public static string RemoveNewLine(this string str)
        {
            if ((str != null) && (!string.IsNullOrEmpty(str)))
            {
                string result = Regex.Replace(HttpUtility.HtmlDecode(str), @"(\r\n?|\n)", "");
                result = Regex.Replace(result, @"\s+", " ");
                result = Regex.Replace(result, @"\<\s*br\s*\/?\s*\>", "");
                return result;
            }
            return str;
        }

        public static string RemoveEmptyLines(this string str)
        {
            if (str == null) return null;
            StringBuilder text_sb = new StringBuilder(str);
            Regex rg_spaces = new Regex(@"(\r\n|\r|\n)([\s]+\r\n|[\s]+\r|[\s]+\n)");
            Match m = rg_spaces.Match(text_sb.ToString());
            while (m.Success)
            {
                text_sb = text_sb.Replace(m.Groups[2].Value, "");
                m = rg_spaces.Match(text_sb.ToString());
            }
            return text_sb.ToString().Trim();
        }

        public static string ReplaceInvalidChars(this string filename)
        {
            if (string.IsNullOrEmpty(filename)) return filename;
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        public static string SafeStringFormat(this string format, params object[] args)
        {
            if (format == null) return null;
            int maxIndex = -1;
            Regex regex = new Regex(@"\{(\d+)\}");
            foreach (Match match in regex.Matches(format))
            {
                int index = int.Parse(match.Groups[1].Value);
                if (index > maxIndex) maxIndex = index;
            }

            if (maxIndex >= 0)
            {
                if (args == null || args.Length <= maxIndex)
                {
                    var newArgs = new object[maxIndex + 1];
                    if (args != null) Array.Copy(args, newArgs, Math.Min(args.Length, newArgs.Length));
                    args = newArgs;
                }
            }

            return string.Format(format, args);
        }

        #endregion

        #region Stream & Array helpers

        public static byte[] ToArray(this Stream input)
        {
            if (input == null) return Array.Empty<byte>();
            // If MemoryStream, use ToArray directly
            if (input is MemoryStream)
            {
                MemoryStream ms = (MemoryStream)input;
                return ms.ToArray();
            }
            using (var msOut = new MemoryStream())
            {
                try { if (input.CanSeek) input.Seek(0, SeekOrigin.Begin); } catch { }
                input.CopyTo(msOut);
                return msOut.ToArray();
            }
        }

        public static Stream ToStream(this string str)
        {
            return str.ToStream(Encoding.UTF8);
        }

        public static Stream ToStream(this string str, Encoding encoding)
        {
            return new MemoryStream(encoding.GetBytes(str ?? ""));
        }

        public static MemoryStream ToMemoryStream(this string path)
        {
            if (!File.Exists(path)) return null;
            var stream = new MemoryStream();
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                fs.CopyTo(stream);
            }
            stream.Position = 0;
            return stream;
        }

        public static void SaveTo(this MemoryStream stream, string path)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            using (var fileStream = System.IO.File.Create(path))
            {
                stream.Seek(0, System.IO.SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }
        }

        #endregion

        #region Currency / numeric format helpers

        // Fix: original used number % 2 > 0 which was wrong; use fractional check.
        public static string ToCurrency(this decimal number)
        {
            if (number % 1 != 0)
                return number.ToString("N2", CultureInfo.InvariantCulture);
            else
                return number.ToString("N0", CultureInfo.InvariantCulture);
        }

        public static string ToString(this string[] str, string separator)
        {
            if (str == null) return string.Empty;
            return string.Join(separator, str);
        }

        // Original ToString(decimal, decimaldigits) kept but fixed behavior
        public static string ToString(this decimal num, int decimaldigits = 2)
        {
            char[] separator = { '.', ',', '/' };
            string[] temp = num.ToString(CultureInfo.InvariantCulture).Split(separator);

            if ((temp.Length > 1) &&
                (!string.IsNullOrEmpty(temp[1])) &&
                (temp[1].IsNumeric()) &&
                (temp[1].Length > decimaldigits))
                return temp[0] + temp[1].Substring(0, decimaldigits).ToSuffix();

            if ((temp.Length > 1) &&
                (!string.IsNullOrEmpty(temp[1])) &&
                (temp[1].IsNumeric()) &&
                (temp[1].Length < decimaldigits))
                return temp[0] + temp[1].ToSuffix();

            return temp[0];
        }

        public static string ToSuffix(this string extention)
        {
            if (string.IsNullOrEmpty(extention)) return extention;
            return "." + extention;
        }

        public static string ToPrefix(this string extention)
        {
            if (string.IsNullOrEmpty(extention)) return extention;
            return extention + ".";
        }

        #endregion

        #region ToDouble/Float/Decimal with culture handling (safe)

        public static double ToDouble(this string str)
        {
            double result = 0;
            if (string.IsNullOrEmpty(str)) return 0;
            if (!str.IsDouble()) return 0;

            if (!double.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result))
            {
                char systemSeparator = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0];
                string s = str.Replace(".", systemSeparator.ToString()).Replace(",", systemSeparator.ToString()).Replace("/", systemSeparator.ToString());
                double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out result);
            }

            return result;
        }

        public static float ToFloat(this string str)
        {
            if (string.IsNullOrEmpty(str)) return 0;
            if (!str.IsDouble()) return 0;

            char systemSeparator = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0];
            string s = str.Replace(".", systemSeparator.ToString()).Replace(",", systemSeparator.ToString()).Replace("/", systemSeparator.ToString());

            float result = 0;
            float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out result);
            return result;
        }

        public static decimal ToDecimal(this string str)
        {
            if (string.IsNullOrEmpty(str)) return 0;
            if (!str.IsDouble()) return 0;

            char systemSeparator = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0];
            string s = str.Replace(".", systemSeparator.ToString()).Replace(",", systemSeparator.ToString()).Replace("/", systemSeparator.ToString());

            decimal result = 0;
            decimal.TryParse(s, NumberStyles.Number, CultureInfo.CurrentCulture, out result);
            return result;
        }

        #endregion

        #region Lists / conversions

        public static List<int> ToIntList(this string str, char separator)
        {
            char[] Separator = { separator };
            List<int> result = new List<int>();
            if ((!string.IsNullOrEmpty(str)) && (str.Contains(separator)))
                result = str.Split(Separator, StringSplitOptions.RemoveEmptyEntries).Select(B => Convert.ToInt32(B)).ToList();
            else if ((!string.IsNullOrEmpty(str)) && (str.IsNumeric()))
                result.Add(Convert.ToInt32(str));
            return result;
        }

        public static List<string> ToList(this string str, char separator)
        {
            char[] Separator = { separator };
            List<string> result = new List<string>();
            if ((!string.IsNullOrEmpty(str)) && (str.Contains(separator)))
                result = str.Split(Separator, StringSplitOptions.RemoveEmptyEntries).ToList();
            else if (!string.IsNullOrEmpty(str))
                result.Add(str);
            return result;
        }

        public static List<int> ToIntList(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return new List<int>();

            var parts = str
                .Split(new char[] { ',', ';', '|', '"', '\'', '\r', '\n', '\t', ' ', '(', ')', '[', ']', '{', '}' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToStandardDigits())
                .Where(s => s.IsNumeric())
                .Select(s => Convert.ToInt32(s))
                .ToList();

            return parts;
        }

        public static List<double> ToDoubleList(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return new List<double>();

            var parts = str
                .Split(new char[] { ',', ';', '|', '"', '\'', '\r', '\n', '\t', ' ', '(', ')', '[', ']', '{', '}' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToStandardDigits())
                .Where(s => s.IsDouble())
                .Select(s => Convert.ToDouble(s))
                .ToList();

            return parts;
        }

        public static List<string> ToStringList(this string str, char separator)
        {
            char[] Separator = { separator };
            List<string> result = new List<string>();
            if (str.Contains(separator))
                result = str.Split(Separator, StringSplitOptions.RemoveEmptyEntries).Select(B => B).ToList();
            else if (str.IsNumeric())
                result.Add(str);
            return result;
        }

        #endregion

        #region JSON helpers

        public static bool IsJson(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            input = input.Trim();
            if ((input.StartsWith("{") && input.EndsWith("}")) || (input.StartsWith("[") && input.EndsWith("]")))
            {
                try
                {
                    JToken.Parse(input);
                    return true;
                }
                catch { return false; }
            }
            return false;
        }

        public static string ToJson(this object source)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    Error = (sender, args) => { args.ErrorContext.Handled = true; },
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Include
                };
                string json = JsonConvert.SerializeObject(source, settings);
                return json;
            }
            catch (Exception ex)
            {
                return $"{ex.Message}";
            }
        }

        public static string ToJson(this DataRow datarow)
        {
            if (datarow == null) return "null";
            var dict = new Dictionary<string, object>();
            foreach (DataColumn col in datarow.Table.Columns)
            {
                dict.Add(col.ColumnName, datarow[col]);
            }
            return JsonConvert.SerializeObject(dict);
        }

        public static string ToJson(this System.Data.DataTable dt, bool ColumnName = false)
        {
            if (dt == null || dt.Rows.Count < 1) return "[]";
            JArray array = new JArray();
            foreach (DataRow dr in dt.Rows)
            {
                JObject item = new JObject();
                int index = 0;
                foreach (DataColumn col in dt.Columns)
                {
                    item.Add((ColumnName ? col.ColumnName : "Cell" + index.ToString()), dr[col.ColumnName]?.ToString());
                    index++;
                }
                array.Add(item);
            }
            return array.ToString(Formatting.Indented);
        }

        public static List<T> ToListof<T>(this JArray array)
        {
            if (array == null) return new List<T>();
            return array.ToObject<List<T>>();
        }

        public static List<T> ToListof<T>(this DataTable dt)
        {
            return dt.ToListof<T>(false);
        }

        public static List<T> ToListof<T>(this DataTable dt, bool isSimpleType)
        {
            if (dt == null || dt.Rows.Count == 0 || dt.Columns.Count == 0) return new List<T>();

            if (isSimpleType)
            {
                Type type = typeof(T);
                return dt.AsEnumerable()
                    .Where(r => r[0] != DBNull.Value)
                    .Select(r => (T)Convert.ChangeType(r[0], type))
                    .ToList();
            }
            else
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
                var columnNames = dt.Columns.Cast<DataColumn>()
                    .Where(c => (c != null) && !string.IsNullOrEmpty(c.ColumnName))
                    .Select(c => c.ColumnName.CleanExcelColumnName().Trim().ToLower())
                    .ToList();
                var objectProperties = typeof(T).GetProperties(flags);
                var targetList = dt.AsEnumerable().Select(dataRow =>
                {
                    var instanceOfT = Activator.CreateInstance<T>();
                    var properties = objectProperties.Where(property =>
                        columnNames.Contains(property.Name.ToLower()) &&
                        (dataRow != null) &&
                        (dataRow[property.Name] != null) &&
                        (dataRow[property.Name] != DBNull.Value)
                        );

                    foreach (var property in properties)
                    {
                        try
                        {
                            var valStr = dataRow[property.Name].ToString().CleanExcelColumnName();
                            if ((property.PropertyType == typeof(Int32)) && (valStr.IsInt32()))
                                property.SetValue(instanceOfT, Convert.ToInt32(valStr), null);

                            else if ((property.PropertyType == typeof(Int64)) && (valStr.IsInt64()))
                                property.SetValue(instanceOfT, Convert.ToInt64(valStr), null);

                            else if ((property.PropertyType == typeof(decimal)) && (valStr.IsDouble()))
                                property.SetValue(instanceOfT, Convert.ToDecimal(valStr), null);

                            else if ((property.PropertyType == typeof(double)) && (valStr.IsDouble()))
                                property.SetValue(instanceOfT, Convert.ToDouble(valStr), null);

                            else if ((property.PropertyType == typeof(float)) && (valStr.IsDouble()))
                                property.SetValue(instanceOfT, Convert.ToSingle(valStr), null);

                            else if ((property.PropertyType == typeof(bool)) && ((valStr.IsBool()) || (valStr.IsNumeric())))
                                property.SetValue(instanceOfT, Convert.ToBoolean(valStr), null);

                            else if (property.PropertyType == typeof(DateTime))
                                property.SetValue(instanceOfT, Convert.ToDateTime(valStr), null);

                            else
                                property.SetValue(instanceOfT, valStr, null);
                        }
                        catch { }
                    }
                    return instanceOfT;
                }).ToList();

                return targetList;
            }
        }

        #endregion

        #region Misc: Hash / Enum / Version / TimeSpan / ToTime / ToAlphabet etc.

        public static string GetHashCode256(this string strData)
        {
            if (strData == null) return null;
            var message = Encoding.UTF8.GetBytes(strData);
            using (var hashString = SHA256.Create())
            {
                var hashValue = hashString.ComputeHash(message);
                var sb = new StringBuilder(hashValue.Length * 2);
                foreach (byte x in hashValue) sb.AppendFormat("{0:x2}", x);
                return sb.ToString();
            }
        }

        public static string GetEnumDescription(this Enum value)
        {
            if (value == null) return null;
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0) return attributes[0].Description;
            else return value.ToString();
        }

        public static string CleanExcelColumnName(this string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            string result = name;
            if (result.StartsWith("\"")) result = result.Substring(1);
            if (result.EndsWith("\"")) result = result.Substring(0, result.Length - 1);
            if (result.StartsWith("'")) result = result.Substring(1);
            if (result.EndsWith("'")) result = result.Substring(0, result.Length - 1);
            return result;
        }

        public static bool IsClass(this PropertyInfo property)
        {
            if (property == null) return false;
            return property.PropertyType.IsClass && !property.PropertyType.FullName.StartsWith("System.");
        }

        public static bool IsList(this PropertyInfo property)
        {
            if (property == null) return false;
            return (property.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(property.PropertyType));
        }

        public static Object ToObject(this PropertyInfo propertyInfo, object parent)
        {
            if (propertyInfo == null || parent == null) return null;
            var source = propertyInfo.GetValue(parent, null);
            var destination = Activator.CreateInstance(propertyInfo.PropertyType);

            if (propertyInfo.IsClass())
            {
                foreach (PropertyInfo prop in destination.GetType().GetProperties().ToList())
                {
                    var srcProp = source.GetType().GetProperty(prop.Name);
                    if (srcProp == null) continue;
                    var value = srcProp.GetValue(source, null);
                    prop.SetValue(destination, value, null);
                }
            }
            else if (propertyInfo.IsList())
            {
                Type type = destination.GetType().GetGenericArguments()[0];
                IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
                object item = Activator.CreateInstance(type);
                list.Add(item);
                return list;
            }

            return destination;
        }

        public static string ToAlphabet(this int number, bool isCaps)
        {
            Char c = (Char)((isCaps ? 65 : 97) + (number - 1));
            return c.ToString();
        }

        public static T ToEnum<T>(this string enumString)
        {
            return (T)Enum.Parse(typeof(T), enumString, true);
        }

        public static T ToEnum<T>(this int enumInt)
        {
            return (T)Enum.Parse(typeof(T), enumInt.ToString());
        }

        public static bool IsVersion(this string str)
        {
            Version ver = null;
            return Version.TryParse(str, out ver);
        }

        public static Version ToVersion(this string str)
        {
            Version ver = null;
            return Version.TryParse(str, out ver) ? ver : null;
        }

        public static TimeSpan? ToTime(this string str)
        {
            TimeSpan result;
            if (TimeSpan.TryParse(str, out result)) return result;
            return null;
        }

        public static int ToAlpha(this float f)
        {
            return (f >= 1.0 ? 255 : (f <= 0.0 ? 0 : (int)Math.Floor(f * 256.0)));
        }

        #endregion

        #region Collections conversion helpers

        public static NameValueCollection ToNameValueCollection(this object val)
        {
            NameValueCollection formFields = new NameValueCollection();
            if (val == null) return formFields;
            foreach (var pi in val.GetType().GetProperties())
            {
                formFields.Add(pi.Name, (pi.GetValue(val, null) ?? "").ToString());
            }
            return formFields;
        }

        public static IDictionary ToDictionary(this Hashtable table)
        {
            if (table == null) return null;
            return table.Cast<DictionaryEntry>().ToDictionary(d => d.Key, d => d.Value);
        }

        public static Hashtable ToHashtable(this NameValueCollection parameters)
        {
            Hashtable result = new Hashtable(StringComparer.OrdinalIgnoreCase);
            if (parameters == null) return result;
            foreach (string key in parameters.AllKeys)
            {
                result.Add(key, parameters[key]);
            }
            return result;
        }

        #endregion

        #region JSON escaping helpers (original ones kept)

        private static bool NeedEscape(string src, int i)
        {
            char c = src[i];
            return c < 32 || c == '"' || c == '\\'
                // Broken lead surrogate
                || (c >= '\uD800' && c <= '\uDBFF' &&
                    (i == src.Length - 1 || src[i + 1] < '\uDC00' || src[i + 1] > '\uDFFF'))
                // Broken tail surrogate
                || (c >= '\uDC00' && c <= '\uDFFF' &&
                    (i == 0 || src[i - 1] < '\uD800' || src[i - 1] > '\uDBFF'))
                // To produce valid JavaScript
                || c == '\u2028' || c == '\u2029'
                // Escape "</" for <script> tags
                || (c == '/' && i > 0 && src[i - 1] == '<');
        }
        public static string JsonEscapingSpecialCharacters(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int start = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (NeedEscape(str, i))
                {
                    sb.Append(str, start, i - start);
                    switch (str[i])
                    {
                        case '\b': sb.Append("\\b"); break;
                        case '\f': sb.Append("\\f"); break;
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        case '\t': sb.Append("\\t"); break;
                        case '\"': sb.Append("\\\""); break;
                        case '\\': sb.Append("\\\\"); break;
                        case '/': sb.Append("\\/"); break;
                        default:
                            sb.Append("\\u");
                            sb.Append(((int)str[i]).ToString("x04"));
                            break;
                    }
                    start = i + 1;
                }
            }
            sb.Append(str, start, str.Length - start);
            return sb.ToString();
        }

        public static string JsonEncodedUnexpectedCharacters(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            string pattern = @"\:\'([^?]*?)(',|'\})";
            RegexOptions regexOptions = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace;
            Regex regex = new Regex(pattern, regexOptions);

            foreach (Match match in regex.Matches(str))
            {
                string content = match.Groups[1].Value;
                if (content.Contains('\''))
                {
                    content = content.Replace("'", "[&#39;]");
                    str = str.Replace(match.Groups[1].Value, content);
                }
            }
            return str;
        }

        public static string JsonDecodeUnexpectedCharacters(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return str.Replace("[&#39;]", "'");
        }

        public static string JsonEncodedText(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            str = Regex.Replace(str, "^\"|\"$", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return Newtonsoft.Json.JsonConvert.ToString(str);
        }

        public static string JsonDecodeText(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            str = Regex.Replace(str, "^\"|\"$", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            str = Regex.Unescape(str);
            return str;
        }

        #endregion

        #region Script/Style removal helpers

        public static string RemoveScript(this string str)
        {
            if ((str != null) && (!string.IsNullOrEmpty(str)))
                str = Regex.Replace(HttpUtility.HtmlDecode(str), @"<script[^>]*>[\s\S]*?</script>", string.Empty, RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            return str;
        }

        public static string RemoveStyle(this string str, bool RemoveAttribute)
        {
            if ((str != null) && (!string.IsNullOrEmpty(str.Trim())))
            {
                if (RemoveAttribute)
                    str = Regex.Replace(HttpUtility.HtmlDecode(str), @"style\s*=\s*('|"")[^\""']*\1", string.Empty, RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

                str = Regex.Replace(HttpUtility.HtmlDecode(str), @"<style[^>]*>[\s\S]*?</style>", string.Empty, RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            }
            return str;
        }

        public static string RemoveHtmlComment(this string str)
        {
            if ((str != null) && (!string.IsNullOrEmpty(str.Trim())))
            {
                str = Regex.Replace(HttpUtility.HtmlDecode(str), @"(?=<!--)([\s\S]*?)-->", string.Empty, RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            }
            return str;
        }

        #endregion

        #region Misc helpers (GetNumbers, GetLetters, Extract names, etc.)

        public static string GetNumerical(this string str, bool throwExceptionIfNull = false)
        {
            if (str == null && !throwExceptionIfNull) return str;
            return Regex.Replace(str ?? "", "[^.0-9]", "");
        }

        public static string GetNumbers(this string str, bool throwExceptionIfNull = false)
        {
            if (str == null && !throwExceptionIfNull) return str;
            return (str == null) ? null : new string(str.Where(c => char.IsDigit(c)).ToArray());
        }

        public static string GetLetters(this string str, bool throwExceptionIfNull = false)
        {
            if (str == null && !throwExceptionIfNull) return str;
            return (str == null) ? null : new string(str.Where(c => char.IsLetter(c)).ToArray());
        }

        public static string GetLettersAndNumbers(this string str, bool throwExceptionIfNull = false)
        {
            if (str == null && !throwExceptionIfNull) return str;
            return (str == null) ? null : new string(str.Where(c => char.IsLetterOrDigit(c)).ToArray());
        }

        public static string GetLettersAndNumbersAndUnderLine(this string str, bool throwExceptionIfNull = false)
        {
            if (str == null && !throwExceptionIfNull) return str;
            return (str == null) ? null : new string(str.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        }

        public static string ExtractFirstName(this string fullName)
        {
            if (!string.IsNullOrEmpty(fullName) && fullName.Contains(" "))
                return fullName.Substring(0, fullName.LastIndexOf(" "));
            else
                return string.Empty;
        }
        public static string ExtractLastName(this string fullName)
        {
            if (!string.IsNullOrEmpty(fullName) && fullName.Contains(" "))
                return fullName.Substring(fullName.LastIndexOf(" ") + 1);
            else
                return string.Empty;
        }

        #endregion

        #region Hijri / date helpers

        public static string ToHijri(this DateTime date)
        {
            System.Globalization.DateTimeFormatInfo DTFormat;
            DTFormat = new System.Globalization.CultureInfo("ar-sa", false).DateTimeFormat;
            DTFormat.Calendar = new System.Globalization.HijriCalendar();
            DTFormat.ShortDatePattern = "yyyy/MM/dd";
            return (date.Date.ToString("f", DTFormat));
        }

        public static DateTime AddWeeks(this DateTime dtOld, int value)
        {
            return dtOld.AddDays((value * 7));
        }
        public static DateTime EquivalentWeekDay(this DateTime dtOld, int DayOfWeek)
        {
            int Today = (int)DateTime.Today.DayOfWeek;
            return DateTime.Today.AddDays(DayOfWeek - Today);
        }
        public static DateTime StartOfWeek(this DateTime dt, System.Globalization.CultureInfo culture)
        {
            int diff = (7 + (dt.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
        public static DateTime EndOfWeek(this DateTime dt, System.Globalization.CultureInfo culture)
        {
            return dt.StartOfWeek(culture).AddDays(7);
        }
        public static DateTime StartOfYear(this DateTime date, System.Globalization.CultureInfo culture)
        {
            if (culture.ToString().ToLower() == "fa-ir")
            {
                System.Globalization.PersianCalendar pc = new System.Globalization.PersianCalendar();
                return pc.ToDateTime(pc.GetYear(DateTime.Now), 1, 1, 0, 0, 1, 0);
            }
            else
            {
                return new DateTime(DateTime.Now.Year, 1, 1);
            }
        }
        public static DateTime EndOfYear(this DateTime date, System.Globalization.CultureInfo culture)
        {
            if (culture.ToString().ToLower() == "fa-ir")
            {
                System.Globalization.PersianCalendar pc = new System.Globalization.PersianCalendar();
                return pc.ToDateTime(pc.GetYear(DateTime.Now) + 1, 1, 1, 0, 0, 1, 0).AddDays(-1);
            }
            else
            {
                return new DateTime(DateTime.Now.Year + 1, 1, 1).AddDays(-1);
            }
        }
        public static DateTime StartOfMonth(this DateTime date, System.Globalization.CultureInfo culture)
        {
            if (culture.ToString().ToLower() == "fa-ir")
            {
                System.Globalization.PersianCalendar pc = new System.Globalization.PersianCalendar();
                int year = pc.GetYear(date);
                int month = pc.GetMonth(date);
                return pc.ToDateTime(year, month, 1, 0, 0, 1, 0);
            }
            else
            {
                return Convert.ToDateTime(DateTime.Now.Year.ToString() + "/" + DateTime.Now.Month.ToString() + "/1");
            }
        }
        public static DateTime EndOfMonth(this DateTime date, System.Globalization.CultureInfo culture)
        {
            if (culture.ToString().ToLower() == "fa-ir")
            {
                System.Globalization.PersianCalendar pc = new System.Globalization.PersianCalendar();
                int year = pc.GetYear(date);
                int month = pc.GetMonth(date);

                if (month < 12) month = month + 1;
                else
                {
                    year = year + 1;
                    month = 1;
                }

                return (pc.ToDateTime(year, month, 1, 0, 0, 1, 0)).AddDays(-1);
            }
            else
            {
                DateTime temp = DateTime.Now;
                if (DateTime.Now.Month < 12)
                    temp = Convert.ToDateTime(DateTime.Now.Year.ToString() + "/" + (DateTime.Now.Month + 1).ToString() + "/1");
                else
                    temp = Convert.ToDateTime((DateTime.Now.Year + 1).ToString() + "/1/1");
                return temp.AddDays(-1);
            }
        }
        public static string Difference(this DateTime date, DateTime toDateTime)
        {
            try
            {
                TimeSpan difference = (toDateTime - date);
                if (difference.Days > 0)
                    return string.Format("{0} Days, {1} Hours, {2} Minutes, {3} Seconds and {4} Milliseconds", difference.Days, difference.Hours, difference.Minutes, difference.Seconds, difference.Milliseconds);
                else if (difference.Hours > 0)
                    return string.Format("{0} Hours, {1} Minutes, {2} Seconds and {3} Milliseconds", difference.Hours, difference.Minutes, difference.Seconds, difference.Milliseconds);
                else if (difference.Minutes > 0)
                    return string.Format("{0} Minutes, {1} Seconds and {2} Milliseconds", difference.Minutes, difference.Seconds, difference.Milliseconds);
                else if (difference.Seconds > 0)
                    return string.Format("{0} Seconds and {1} Milliseconds", difference.Seconds, difference.Milliseconds);
                else if (difference.Milliseconds > 0)
                    return string.Format("{0} Milliseconds", difference.Milliseconds);
                else
                    return "0";
            }
            catch { return "-1"; }
        }
        public static double GetMillisecondsUntil(this DateTime date, DateTime toDateTime)
        {
            try { return (toDateTime - date).TotalMilliseconds; }
            catch { return -1; }
        }

        #endregion

        #region Replace with StringComparison (original large method kept)

        [System.Diagnostics.DebuggerStepThrough]
        public static string Replace(this string str, string oldValue, string @newValue, StringComparison comparisonType)
        {
            if ((string.IsNullOrEmpty(str)) || (string.IsNullOrEmpty(oldValue)) || (@newValue == null))
            {
                return str;
            }

            StringBuilder resultStringBuilder = new StringBuilder(str.Length);
            bool isReplacementNullOrEmpty = string.IsNullOrEmpty(@newValue);

            const int valueNotFound = -1;
            int foundAt;
            int startSearchFromIndex = 0;
            while ((foundAt = str.IndexOf(oldValue, startSearchFromIndex, comparisonType)) != valueNotFound)
            {
                int charsUntilReplacment = foundAt - startSearchFromIndex;
                if (charsUntilReplacment != 0)
                {
                    resultStringBuilder.Append(str, startSearchFromIndex, charsUntilReplacment);
                }
                if (!isReplacementNullOrEmpty)
                {
                    resultStringBuilder.Append(@newValue);
                }
                startSearchFromIndex = foundAt + oldValue.Length;
                if (startSearchFromIndex == str.Length)
                {
                    return resultStringBuilder.ToString();
                }
            }

            int charsUntilStringEnd = str.Length - startSearchFromIndex;
            resultStringBuilder.Append(str, startSearchFromIndex, charsUntilStringEnd);

            return resultStringBuilder.ToString();
        }

        #endregion

        #region Between (IComparable helper)

        public static bool Between(this IComparable value, IComparable lowerBoundary, IComparable upperBoundary,
            bool includeLowerBoundary = true, bool includeUpperBoundary = true)
        {
            try
            {
                var lower = value.CompareTo(lowerBoundary);
                var upper = value.CompareTo(upperBoundary);
                return (lower > 0 || (includeLowerBoundary && lower == 0)) &&
                       (upper < 0 || (includeUpperBoundary && upper == 0));
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region DictionaryOfPropertiesFromInstance

        public static Dictionary<string, PropertyInfo> DictionaryOfPropertiesFromInstance(this object InstanceOfAType)
        {
            if (InstanceOfAType == null) return null;
            Type TheType = InstanceOfAType.GetType();
            PropertyInfo[] Properties = TheType.GetProperties();
            Dictionary<string, PropertyInfo> PropertiesMap = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo Prop in Properties)
            {
                PropertiesMap.Add(Prop.Name, Prop);
            }
            return PropertiesMap;
        }

        #endregion

        #region ToSQLListParam (TVP) & ToDataTable (from JSON)

        public static DataTable ToSQLListParam(this List<int> param)
        {
            DataTable tvp = new DataTable();
            tvp.Columns.Add(new DataColumn("ID", typeof(int)));
            if (param == null) return tvp;
            foreach (var id in param) tvp.Rows.Add(id);
            return tvp;
        }

        public static DataTable ToDataTable(this string json)
        {
            try
            {
                var jsonLinq = JObject.Parse(json);
                var linqArray = jsonLinq.Descendants().Where(x => x is JArray).FirstOrDefault() as JArray;
                if (linqArray == null) return null;
                var jsonArray = new JArray();
                foreach (JObject row in linqArray.Children<JObject>())
                {
                    var createRow = new JObject();
                    foreach (JProperty column in row.Properties())
                    {
                        if (column.Value is JValue)
                            createRow.Add(column.Name, column.Value);
                    }
                    jsonArray.Add(createRow);
                }
                DataTable temp = JsonConvert.DeserializeObject<DataTable>(jsonArray.ToString());
                return temp;
            }
            catch (Exception ex)
            {
                try { DotNetNuke.Services.Exceptions.Exceptions.LogException(ex); } catch { }
                return null;
            }
        }

        #endregion

        #region GetPropertyValue helper (existing already included above)

        public static object GetPropertyValue2(this object obj, string propertyName)
        {
            // duplicate safe method (kept for completeness if original named differently)
            return GetPropertyValue(obj, propertyName);
        }

        #endregion

        #region Request helpers (GetRequestParameters / GetIPAddress)

        public static string GetRequestParameters(this HttpContext context)
        {
            var dict = new Dictionary<string, string>();
            try
            {
                if (context?.Request?.QueryString != null)
                {
                    foreach (string key in context.Request.QueryString)
                    {
                        if (string.IsNullOrEmpty(key)) continue;
                        var value = context.Request.QueryString[key];
                        if (!string.IsNullOrEmpty(value)) dict[key] = value;
                    }
                }

                if (context?.Request?.HttpMethod == "POST" && context.Request.Form != null)
                {
                    foreach (string key in context.Request.Form)
                    {
                        if (string.IsNullOrEmpty(key)) continue;
                        var value = context.Request.Form[key];
                        if (!string.IsNullOrEmpty(value)) dict[key] = value;
                    }
                }
            }
            catch (Exception ex)
            {
                dict["error"] = ex.Message;
            }

            return JsonConvert.SerializeObject(dict, Formatting.Indented);
        }

        public static string GetIPAddress(this HttpRequest request)
        {
            if (request == null) return null;
            string forwarded = request.Headers["Forwarded"];
            if (!String.IsNullOrEmpty(forwarded))
            {
                foreach (string segment in forwarded.Split(',')[0].Split(';'))
                {
                    string[] pair = segment.Trim().Split('=');
                    if (pair.Length == 2 && pair[0].Equals("for", StringComparison.OrdinalIgnoreCase))
                    {
                        string ip = pair[1].Trim('"');
                        int left = ip.IndexOf('['), right = ip.IndexOf(']');
                        if (left == 0 && right > 0) return ip.Substring(1, right - 1);
                        int colon = ip.IndexOf(':');
                        if (colon != -1) return ip.Substring(0, colon);
                        return ip;
                    }
                }
            }
            string xForwardedFor = request.Headers["X-Forwarded-For"];
            if (!String.IsNullOrEmpty(xForwardedFor))
            {
                return xForwardedFor.Split(',')[0];
            }
            return request.UserHostAddress;
        }

        #endregion

        #region Misc: Base helpers & secure functions (DNN-dependent parts preserved)

        public static string ReplaceParameters(this string query, List<string> Parameters)
        {
            if ((!string.IsNullOrEmpty(query)) &&
                (Parameters != null) &&
                (Parameters.Count > 0))
            {
                for (int index = 0; index < Parameters.Count; index++)
                {
                    string ParamValue = Parameters[index];
                    string value = $"'{ParamValue}'";

                    if ((value.IsInt32()) || (value.IsInt64()) || (value.IsDouble()))
                        value = ParamValue;

                    query = query.Replace("@p" + index.ToString(), value);
                }
            }

            return query;
        }

        public static string GetNumericalOriginal(this string str, bool throwExceptionIfNull = false)
        {
            return Regex.Replace(str ?? "", "[^.0-9]", "");
        }

        public static string ToStandardDigits(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            if (str.Trim().Length == 0) return str;
            var englishnumbers = new Dictionary<string, string>(){
                {"۰","0" }, {"۱","1" }, {"۲","2" }, {"۳","3" },{"۴","4" }, {"۵","5" },{"۶","6" }, {"۷","7" },{"۸","8" }, {"۹","9" },
                {"٠","0" }, {"١","1" }, {"٢","2" }, {"٣","3" },{"٤","4" }, {"٥","5" },{"٦","6" }, {"٧","7" },{"٨","8" }, {"٩","9" },
            };
            string s = str;
            foreach (var numbers in englishnumbers)
                s = s.Replace(numbers.Key, numbers.Value);
            return s;
        }

        #endregion

        #region ProfanityFilter & Secure (DotNetNuke dependent - original preserved)

        public static string ProfanityFilter(this string source, DotNetNuke.Entities.Portals.PortalSettings PortalSettings, string newValue = "")
        {
            try
            {
                string listName = "ProfanityFilter";
                DotNetNuke.Common.Lists.ListController listController = new DotNetNuke.Common.Lists.ListController();
                IEnumerable<DotNetNuke.Common.Lists.ListEntryInfo> listEntryHostInfos;
                IEnumerable<DotNetNuke.Common.Lists.ListEntryInfo> listEntryPortalInfos;
                List<string> words = new List<string>();

                listEntryHostInfos = listController.GetListEntryInfoItems(listName, string.Empty, DotNetNuke.Common.Utilities.Null.NullInteger);
                listEntryPortalInfos = listController.GetListEntryInfoItems(listName + "-" + PortalSettings.PortalId, string.Empty, PortalSettings.PortalId);

                if ((listEntryHostInfos != null) && (listEntryHostInfos.Count() > 0))
                    words.AddRange(listEntryHostInfos.Select(E => E.Value).ToList());

                if ((listEntryPortalInfos != null) && (listEntryPortalInfos.Count() > 0))
                    words.AddRange(listEntryPortalInfos.Select(E => E.Value).ToList());

                if (string.IsNullOrEmpty(newValue)) newValue = "";

                if ((words != null) && (words.Count > 0))
                {
                    words = words.OrderByDescending(W => W.Length).ToList();
                    words.ForEach(W =>
                    {
                        if (!string.IsNullOrEmpty(W))
                            source = source.Replace(W, newValue);
                    });
                }
            }
            catch { }
            return source;
        }

        public static string InputFilter(this string source, DotNetNuke.Security.PortalSecurity.FilterFlag filter = DotNetNuke.Security.PortalSecurity.FilterFlag.NoSQL)
        {
            return (new DotNetNuke.Security.PortalSecurity()).InputFilter(source, filter);
        }

        public static string Secure(this string str, bool RemoveAttribute)
        {
            try
            {
                if (!string.IsNullOrEmpty(str))
                {
                    str = str.RemoveScript();
                    str = str.RemoveStyle(RemoveAttribute);
                    str = str.RemoveHtmlComment();
                    str = str.InputFilter(DotNetNuke.Security.PortalSecurity.FilterFlag.NoSQL);
                    str = str.InputFilter(DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting);
                    str = str.InputFilter(DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup);
                }
            }
            catch (Exception ex)
            {
                try { DotNetNuke.Services.Exceptions.Exceptions.LogException(ex); } catch { }
            }
            return str;
        }

        public static string Secure(this string str, DotNetNuke.Entities.Portals.PortalSettings PortalSettings, bool RemoveAttribute)
        {
            try
            {
                if (!string.IsNullOrEmpty(str))
                {
                    str = str.RemoveScript();
                    str = str.RemoveStyle(RemoveAttribute);
                    str = str.RemoveHtmlComment();
                    str = str.InputFilter(DotNetNuke.Security.PortalSecurity.FilterFlag.NoSQL);
                    str = str.InputFilter(DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting);
                    str = str.InputFilter(DotNetNuke.Security.PortalSecurity.FilterFlag.NoMarkup);
                    str = str.ProfanityFilter(PortalSettings, "");
                }
            }
            catch { }
            return str;
        }

        public static string Secure(this string str)
        {
            try
            {
                if (!string.IsNullOrEmpty(str))
                {
                    str = HttpUtility.UrlDecode(str);
                    str = str.RemoveScript();
                    str = str.RemoveStyle(true);
                    str = str.RemoveHtmlComment();
                    str = str.InputFilter(DotNetNuke.Security.PortalSecurity.FilterFlag.NoSQL);
                    str = str.InputFilter(DotNetNuke.Security.PortalSecurity.FilterFlag.NoScripting);
                    str = str.Trim();
                }
            }
            catch (Exception ex)
            {
                try { DotNetNuke.Services.Exceptions.Exceptions.LogException(ex); } catch { }
            }
            return str;
        }

        #endregion

        #region CSharpName + primitives map (restored)

        public static string CSharpName(this Type type)
        {
            if (type == null) return null;
            string result;
            if (primitiveTypes.TryGetValue(type, out result))
                return result;
            else
                result = type.Name.Replace('+', '.');

            if (!type.IsGenericType)
                return result;
            else if (type.IsNested && type.DeclaringType.IsGenericType)
                throw new NotImplementedException();

            result = result.Substring(0, result.IndexOf("`"));
            return result + "<" + string.Join(", ", type.GetGenericArguments().Select(CSharpName)) + ">";
        }

        private static Dictionary<Type, string> primitiveTypes = new Dictionary<Type, string>
        {
            { typeof(bool), "boolean" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "integer" },
            { typeof(long), "long" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(string), "string" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(ushort), "ushort" },
        };

        #endregion

        #region MapPath / ResolveUrl (original implementations preserved, with fallback)

        public static string MapPath(this string str)
        {
            string temp = string.Empty;
            if (string.IsNullOrEmpty(str)) return str;

            str = str.Replace(@"\", "/");
            var lower = str.ToLowerInvariant();
            if (lower.Contains("/portals/")) str = str.Substring(lower.IndexOf("/portals/"));
            else if (lower.Contains("portals/")) str = str.Substring(lower.IndexOf("portals/"));
            if (lower.Contains("/desktopmodules/")) str = str.Substring(lower.IndexOf("/desktopmodules/"));
            else if (lower.Contains("desktopmodules/")) str = str.Substring(lower.IndexOf("desktopmodules/"));

            if (str.StartsWith("/")) str = str.Substring(1);
            if (str.StartsWith("\\")) str = str.Substring(1);

            try
            {
                if (string.IsNullOrEmpty(temp))
                    temp = HttpContext.Current.Server.MapPath("~/" + str);
            }
            catch { }

            try
            {
                if (string.IsNullOrEmpty(temp))
                {
                    string ApplicationMapPath = DotNetNuke.Common.Globals.ApplicationMapPath;
                    if (!ApplicationMapPath.EndsWith(@"\")) ApplicationMapPath = ApplicationMapPath + @"\";
                    temp = ApplicationMapPath + str.Replace("/", @"\");
                }
            }
            catch { }

            return temp;
        }

        public static string ResolveUrl(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            str = str.Replace(@"\", "/");
            var lower = str.ToLowerInvariant();
            if (lower.Contains("/portals/")) str = str.Substring(lower.IndexOf("/portals/"));
            else if (lower.Contains("portals/")) str = str.Substring(lower.IndexOf("portals/"));
            if (lower.Contains("/desktopmodules/")) str = str.Substring(lower.IndexOf("/desktopmodules/"));
            else if (lower.Contains("desktopmodules/")) str = str.Substring(lower.IndexOf("desktopmodules/"));

            if (str.StartsWith("/")) str = str.Substring(1);
            str = "~/" + str;
            return VirtualPathUtility.ToAbsolute(str);
        }

        public static string ToAbsoluteUrl(this string relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl)) return relativeUrl;

            if (relativeUrl.IsEmail()) return relativeUrl;
            if (relativeUrl.StartsWith("http")) return relativeUrl;
            if (relativeUrl.ToLower().StartsWith("www.")) return "http://" + relativeUrl;

            string absoluteUrl = VirtualPathUtility.ToAbsolute("~/");

            if (!relativeUrl.StartsWith(absoluteUrl))
            {
                if (relativeUrl.StartsWith("/")) relativeUrl = relativeUrl.Insert(0, "~");
                if (!relativeUrl.StartsWith("~/")) relativeUrl = relativeUrl.Insert(0, "~/");
                relativeUrl = VirtualPathUtility.ToAbsolute(relativeUrl);
            }

            if (HttpContext.Current == null) return relativeUrl;

            Uri url = HttpContext.Current.Request.Url;
            string port = ((url.Port != 80) && (url.Scheme == "http")) || ((url.Port != 443) && (url.Scheme == "https")) ? (":" + url.Port) : String.Empty;
            string Domain = String.Format("{0}://{1}{2}", url.Scheme, url.Host, port);

            DotNetNuke.Entities.Portals.PortalSettings PortalSettings = null;
            try { PortalSettings = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(); } catch { }
            if (PortalSettings != null && !string.IsNullOrEmpty(PortalSettings.DefaultPortalAlias))
                Domain = String.Format("{0}://{1}", url.Scheme, PortalSettings.DefaultPortalAlias);

            return String.Format("{0}{1}", Domain, relativeUrl);
        }

        public static string RawUrlCleaner(this string RawUrl)
        {
            if (!string.IsNullOrEmpty(RawUrl))
            {
                if (RawUrl.Contains("?"))
                    RawUrl = RawUrl.Substring(0, RawUrl.IndexOf("?"));
            }
            return RawUrl;
        }

        public static string RemoveVertion(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return str.Contains("?") ? str.Substring(0, str.IndexOf("?")) : str;
        }

        #endregion

        #region JSON decode helpers previously present

        public static string JsonEncodedTextOld(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            str = Regex.Replace(str, "^\"|\"$", "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return JsonConvert.ToString(str);
        }

        #endregion

        #region Awesome<T> original preserved (safer null checks + logging)

        public static void Awesome<T>(this T myObject) where T : class
        {
            if (myObject == null) return;
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (var info in properties)
            {
                try
                {
                    var val = info.GetValue(myObject, null);
                    if ((info.PropertyType == typeof(string)) && (info.CanWrite) && ((val == null) || (val.ToString() == "null")))
                    {
                        info.SetValue(myObject, String.Empty, null);
                    }
                    else if ((info.PropertyType == typeof(int)) && (info.CanWrite) && (val == null))
                    {
                        info.SetValue(myObject, 0, null);
                    }
                    else if ((info.PropertyType == typeof(DateTime)) && (info.CanWrite))
                    {
                        if (val == null)
                            info.SetValue(myObject, (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue, null);
                        else
                        {
                            string temp = val.ToString();
                            DateTime dtemp;
                            if (!DateTime.TryParse(temp, out dtemp))
                                info.SetValue(myObject, (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue, null);
                            else if (dtemp < (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue)
                                info.SetValue(myObject, (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue, null);
                        }
                    }
                    else if ((info.PropertyType == typeof(Object)) && (info.PropertyType != null))
                    {
                        // Not meaningful here; original attempted recursive call; keep as no-op to avoid infinite recursion
                    }
                }
                catch (Exception ex)
                {
                    try { DotNetNuke.Services.Exceptions.Exceptions.LogException(ex); } catch { }
                }
            }
        }

        #endregion

        #region Enum helpers & GetAll

        public static IDictionary<int, string> GetAll<TEnum>() where TEnum : struct
        {
            var enumerationType = typeof(TEnum);
            if (!enumerationType.IsEnum) throw new ArgumentException("Enumeration type is expected.");
            var dictionary = new Dictionary<int, string>();
            foreach (int value in Enum.GetValues(enumerationType))
            {
                var name = Enum.GetName(enumerationType, value);
                dictionary.Add(value, name);
            }
            return dictionary;
        }

        #endregion

        #region HumanFileSize (additions) & ToTime etc.

        public static string HumanFileSize(this int byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0) return "0" + suf[0];
            long bytes = Math.Abs((long)byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }
        public static string HumanFileSize(this long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0) return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        #endregion

    }
}