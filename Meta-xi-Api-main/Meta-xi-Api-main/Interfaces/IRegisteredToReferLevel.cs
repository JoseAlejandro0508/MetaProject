namespace Meta_xi.Application;
public interface IRegisteredToReferLevel
{
    public Task VerifyToReferLevel1(string UniqueCodeReferrer, string UniqueCodeReFerred);
    public Task VerifyToReferLevel2(string UniqueCodeReferrer, string UniqueCodeReFerred);
    public Task VerifyToReferLevel3(string UniqueCodeReferrer, string UniqueCodeReFerred);
}