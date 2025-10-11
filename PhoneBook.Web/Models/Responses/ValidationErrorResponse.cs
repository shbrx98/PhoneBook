namespace PhoneBook.Web.Models.Responses
{
    public class ValidationErrorResponse
    {
        public Dictionary<string, List<string>> Errors { get; set; } = new Dictionary<string, List<string>>();

        public void AddError(string field, string message)
        {
            if (!Errors.ContainsKey(field))
            {
                Errors[field] = new List<string>();
            }
            Errors[field].Add(message);
        }

        public bool HasErrors => Errors.Any();
    }
}