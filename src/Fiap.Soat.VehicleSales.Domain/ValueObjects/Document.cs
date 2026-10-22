namespace Fiap.Soat.VehicleSales.Domain.ValueObjects;

public record Document
{
    private Document() { }

    private Document(string value)
    {
        Value = value;
    }

    public string Value { get; private set; } = string.Empty;

    public bool IsValid() => !string.IsNullOrEmpty(Value) && (IsValidCpf(Value) || IsValidCnpj(Value));

    public static implicit operator Document(string value) => new Document(value);
    public static implicit operator string(Document document) => document.Value;

    private static bool IsValidCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;

        cpf = new string(cpf.Where(char.IsDigit).ToArray());

        if (cpf.Length != 11) return false;
        if (cpf.Distinct().Count() == 1) return false;

        int[] multiplierOne = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multiplierTwo = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

        string tempCpf = cpf[..9];
        int sum = 0;

        for (int i = 0; i < 9; i++) sum += int.Parse(tempCpf[i].ToString()) * multiplierOne[i];

        int remainder = sum % 11;
        int digitOne = remainder < 2 ? 0 : 11 - remainder;

        tempCpf += digitOne;
        sum = 0;

        for (int i = 0; i < 10; i++) sum += int.Parse(tempCpf[i].ToString()) * multiplierTwo[i];

        remainder = sum % 11;
        int digitTwo = remainder < 2 ? 0 : 11 - remainder;

        return cpf.EndsWith($"{digitOne}{digitTwo}");
    }

    private static bool IsValidCnpj(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj)) return false;

        cnpj = new string(cnpj.Where(char.IsDigit).ToArray());

        if (cnpj.Length != 14) return false;
        if (cnpj.Distinct().Count() == 1) return false;

        int[] multiplierOne = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multiplierTwo = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        string tempCnpj = cnpj[..12];
        int sum = 0;

        for (int i = 0; i < 12; i++) sum += int.Parse(tempCnpj[i].ToString()) * multiplierOne[i];

        int remainder = sum % 11;
        int digitOne = remainder < 2 ? 0 : 11 - remainder;

        tempCnpj += digitOne;
        sum = 0;

        for (int i = 0; i < 13; i++) sum += int.Parse(tempCnpj[i].ToString()) * multiplierTwo[i];

        remainder = sum % 11;
        int digitTwo = remainder < 2 ? 0 : 11 - remainder;

        return cnpj.EndsWith($"{digitOne}{digitTwo}");
    }
}
