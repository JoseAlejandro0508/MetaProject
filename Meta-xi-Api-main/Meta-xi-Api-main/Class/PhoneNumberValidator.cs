using PhoneNumbers;

namespace Meta_xi.Application;
public class PhoneNumberValidator
{
    public bool IsValidPhoneNumber (string PhoneNumber){
        var phoneUtil = PhoneNumberUtil.GetInstance();
        try
        {
            var number = phoneUtil.Parse(PhoneNumber, null);
            return phoneUtil.IsValidNumber(number);
        }
        catch (System.Exception)
        {
            return false;
        }
    }
}