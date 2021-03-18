using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kbs.IdoWeb.Api.Localization
{
	public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
	{
		public override IdentityError ConcurrencyFailure() { return new IdentityError { Code = nameof(ConcurrencyFailure), Description = "Fehler bzgl. der optimistischen Nebenläufigkeit, das Objekt wurde verändert." }; }
		public override IdentityError DefaultError() { return new IdentityError { Code = nameof(DefaultError), Description = $"Ein unbekannter Fehler ist aufgetreten." }; }
		public override IdentityError DuplicateEmail(string email) { return new IdentityError { Code = nameof(DuplicateEmail), Description = $"Die E-Mail '{email}' wird bereits verwendet." }; }
		public override IdentityError DuplicateRoleName(string role) { return new IdentityError { Code = nameof(DuplicateRoleName), Description = $"Der Rollenname '{role}' wird bereits verwendet." }; }
		public override IdentityError DuplicateUserName(string userName) { return new IdentityError { Code = nameof(DuplicateUserName), Description = $"Der Nutzername '{userName}' wird bereits verwendet." }; }
		public override IdentityError InvalidEmail(string email) { return new IdentityError { Code = nameof(InvalidEmail), Description = $"Die E-Mail '{email}' ist ungültig." }; }
		public override IdentityError InvalidRoleName(string role) { return new IdentityError { Code = nameof(InvalidRoleName), Description = $"Der Rollenname '{role}' ist ungültig." }; }
		public override IdentityError InvalidToken() { return new IdentityError { Code = nameof(InvalidToken), Description = "Ungültiges Token." }; }
		public override IdentityError InvalidUserName(string userName) { return new IdentityError { Code = nameof(InvalidUserName), Description = $"Der Nutzername '{userName}' ist ungültig. Er darf nur Buchstaben und Ziffern enthalten." }; }
		public override IdentityError LoginAlreadyAssociated() { return new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = "Es ist bereits ein Nutzer mit diesem Login vorhanden." }; }
		public override IdentityError PasswordMismatch() { return new IdentityError { Code = nameof(PasswordMismatch), Description = "Falsches Kennwort." }; }
		public override IdentityError PasswordRequiresDigit() { return new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "Kennwörter müssen mindestens eine Zahl ('0'-'9') enthalten" }; }
		public override IdentityError PasswordRequiresLower() { return new IdentityError { Code = nameof(PasswordRequiresLower), Description = "Kennwörter müssen mindestens einen Kleinbuchstaben ('a'-'z') enthalten" }; }
		public override IdentityError PasswordRequiresNonAlphanumeric() { return new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Kennwörter müssen mindestens ein Sonderzeichen enthalten." }; }
		public override IdentityError PasswordRequiresUpper() { return new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "Kennwörter müssen mindestens einen Großbuchstaben('A'-'Z') enthalten." }; }
		public override IdentityError PasswordTooShort(int length) { return new IdentityError { Code = nameof(PasswordTooShort), Description = $"Kennwörter müssen mindestens {length} Zeichen lang sein." }; }
		public override IdentityError UserAlreadyHasPassword() { return new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = "Für den Nutzer ist bereits ein Kennwort festgelegt." }; }
		public override IdentityError UserAlreadyInRole(string role) { return new IdentityError { Code = nameof(UserAlreadyInRole), Description = $"Der Nutzer ist bereits Mitglied der Rolle '{role}'." }; }
		public override IdentityError UserLockoutNotEnabled() { return new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = "Aussperrung ist für diesen Nutzer nicht aktiviert." }; }
		public override IdentityError UserNotInRole(string role) { return new IdentityError { Code = nameof(UserNotInRole), Description = $"Der Nutzer ist nicht in der Rolle '{role}' enthalten." }; }
	}
}
