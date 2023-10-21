namespace KFADynamics.DataEntry.Business.Classes;

public class CurrencyToWordsConverter
{
  public static string ConvertToWords(object value)
  {
    var numb = value.ToString();
    if (string.IsNullOrEmpty(numb)
        || Convert.ToDecimal(numb) == 0)
      return "";

    string val = "", wholeNo = numb;
    string? andStr = "", pointStr = "";
    var endStr = "Only";
    try
    {
      var decimalPlace = numb.IndexOf(".", StringComparison.InvariantCultureIgnoreCase);
      if (decimalPlace > 0)
      {
        wholeNo = numb.Substring(0, decimalPlace);
        var points = numb.Substring(decimalPlace + 1);

        if (Convert.ToInt32(points) > 0)
        {
          if (points.Length != 2)
            points = Convert.ToDecimal(numb).ToString("F2").Substring(decimalPlace + 1);

          andStr = " And "; // just to separate whole numbers from points/cents
          endStr = "Cents " + endStr; //Cents
          pointStr = ConvertWholeNumber(points[..2]);
        }
      }

      val = string.Format("{0} Shillings{1}{2} {3}", ConvertWholeNumber(wholeNo)?.Trim(), andStr, pointStr, endStr)
          .Replace(", Hundred And", ",");
    }
    catch
    {
      // ignored
    }

    return val == "Shillings Only" ? "" : val;
  }

  private static string? ConvertWholeNumber(string? number)
  {
    var word = "";
    try
    {
      if (string.IsNullOrWhiteSpace(number))
        return null;

      var isDone = false; //test if already translated
      var dblAmt = Convert.ToDouble(number);
      // if ((dblAmt > 0) && number.StartsWith("0"))
      if (dblAmt > 0)
      {
        // test for zero or digit zero in a nuemric

        var numDigits = number.Length;
        var pos = 0; //store digit grouping
        var place = ","; //digit grouping name:hundres,thousand,etc...
        switch (numDigits)
        {
          case 1: //ones' range
            word = Ones(number);
            isDone = true;
            break;

          case 2: //tens' range
            word = Tens(number);
            isDone = true;
            break;

          case 3: //hundreds' range
            pos = numDigits % 3 + 1;
            place = number.EndsWith("00") ? " Hundred " : " Hundred And ";
            break;

          case 4: //thousands' range
          case 5:
          case 6:
            pos = numDigits % 4 + 1;
            place = " Thousand, ";
            break;

          case 7: //millions' range
          case 8:
          case 9:
            pos = numDigits % 7 + 1;
            place = " Million, ";
            break;

          case 10: //Billions's range
          case 11:
          case 12:

            pos = numDigits % 10 + 1;
            place = " Billion, ";
            break;

          case 13: //Billions's range
          case 14:
          case 15:

            pos = numDigits % 13 + 1;
            place = " Trillion, ";
            break;
          //add extra case options for anything above Billion...
          default:
            isDone = true;
            break;
        }

        if (!isDone)
        {
          // if transalation is not done, continue...(Recursion comes in now!!)
          if (number.Substring(0, pos) != "0" && number.Substring(pos) != "0")
          {
            try
            {
              word = ConvertWholeNumber(number.Substring(0, pos)) + place +
                     ConvertWholeNumber(number.Substring(pos));
            }
            catch
            {
              // ignored
            }
          }
          else
          {
            word = ConvertWholeNumber(number.Substring(0, pos)) + place +
                   ConvertWholeNumber(number.Substring(pos));
          }

          // check for trailing zeros
          // if (beginsZero) word = " and " + word.Trim();
        }

        // ignore digit grouping names
        if (word?.Trim()?.Equals(place?.Trim()) ?? false) word = "";
      }
    }
    catch
    {
      // ignored
    }

    word = word?.Trim();
    if (word?.EndsWith(",") ?? false)
      word = word?[..^1];
    return word;
  }

  private static string? Tens(string number)
  {
    var xNumber = Convert.ToInt32(number);
    string? name = null;
    switch (xNumber)
    {
      case 10:
        name = "Ten";
        break;

      case 11:
        name = "Eleven";
        break;

      case 12:
        name = "Twelve";
        break;

      case 13:
        name = "Thirteen";
        break;

      case 14:
        name = "Fourteen";
        break;

      case 15:
        name = "Fifteen";
        break;

      case 16:
        name = "Sixteen";
        break;

      case 17:
        name = "Seventeen";
        break;

      case 18:
        name = "Eighteen";
        break;

      case 19:
        name = "Nineteen";
        break;

      case 20:
        name = "Twenty";
        break;

      case 30:
        name = "Thirty";
        break;

      case 40:
        name = "Fourty";
        break;

      case 50:
        name = "Fifty";
        break;

      case 60:
        name = "Sixty";
        break;

      case 70:
        name = "Seventy";
        break;

      case 80:
        name = "Eighty";
        break;

      case 90:
        name = "Ninety";
        break;

      default:
        if (xNumber > 0)
        {
          name = Tens(number.Substring(0, 1) + "0") + " " + Ones(number.Substring(1));
        }

        break;
    }

    return name;
  }

  private static string Ones(string ogrNumber)
  {
    var number = Convert.ToInt32(ogrNumber);
    var name = "";
    switch (number)
    {
      case 1:
        name = "One";
        break;

      case 2:
        name = "Two";
        break;

      case 3:
        name = "Three";
        break;

      case 4:
        name = "Four";
        break;

      case 5:
        name = "Five";
        break;

      case 6:
        name = "Six";
        break;

      case 7:
        name = "Seven";
        break;

      case 8:
        name = "Eight";
        break;

      case 9:
        name = "Nine";
        break;
    }

    return name;
  }

  private static string ConvertDecimals(string number)
  {
    return number.Select(t => t.ToString()).Select(digit => digit.Equals("0") ? "Zero" : Ones(digit)).Aggregate("", (current, engOne) => string.Format("{0} {1}", current, engOne));
  }
}
