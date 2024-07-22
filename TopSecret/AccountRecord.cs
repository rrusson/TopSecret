namespace TopSecret
{
	//? Make this a Record instead ?

	public class AccountRecord
	{
		public Guid? Id { get; private set; } = Guid.NewGuid();

		public string? AccountName { get; set; }

		public string? UserName { get; set; }

		public string? Password { get; set; }

		public string? Url { get; set; }

		public AccountRecord() { }

		/// <summary>
		/// Creates a populated account object
		/// </summary>
		/// <param name="accountName">Resource Name</param>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <param name="url">Site URL</param>
		public AccountRecord(string? accountName, string? userName, string? password, string? url)
		{
			this.AccountName = accountName;
			this.UserName = userName;
			this.Password = password;
			this.Url = url;
		}

		/// <summary>
		/// Creates a populated account object
		/// </summary>
		/// <param name="accountData">A delimited string containing account data</param>
		public AccountRecord(string accountData)
		{
			string[] acctItem = accountData.Split((char)9);
			if (acctItem.Count() < 5)
			{
				return;
			}

			this.Id = (Guid.TryParse(acctItem[0], out Guid acctId) ? acctId : Guid.NewGuid());
			this.AccountName = acctItem[1];
			this.UserName = acctItem[2];
			this.Password = acctItem[3];
			this.Url = acctItem[4];
		}

		public override string ToString()
		{
			return $"{Id}{(char)9}{AccountName}{(char)9}{UserName}{(char)9}{Password}{(char)9}{Url}";
		}
	}
}

