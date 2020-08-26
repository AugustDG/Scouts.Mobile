namespace Scouts.Models
{
    public class ChatUserModel : UserDataModel
    {
        string firstLetter;

        public string FirstLetter
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(firstLetter))
                    return firstLetter.ToLowerInvariant();

                firstLetter = Username?.Length > 0 ? Username[0].ToString() : "?";
                return firstLetter.ToLowerInvariant();
            }
            set => firstLetter = value;
        }
    }
}