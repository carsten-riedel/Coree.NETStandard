using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Coree.NETStandard.Classes.CommonDistinguishedNameBuilder
{
    /// <summary>
    /// A builder class for creating a distinguished name string with various attributes, supporting immutability after building.
    /// </summary>
    public class CommonDistinguishedNameBuilder
    {
        private readonly List<string> _attributes = new List<string>();
        private bool _isBuilt = false;

        /// <summary>
        /// Adds a generic attribute to the distinguished name.
        /// </summary>
        /// <param name="key">The attribute type (e.g., "CN", "O").</param>
        /// <param name="value">The value of the attribute.</param>
        /// <exception cref="InvalidOperationException">Thrown if the builder has already been used to build a DN.</exception>
        /// <exception cref="ArgumentException">Thrown if the key or value are null or whitespace.</exception>
        public void AddAttribute(string key, string value)
        {
            if (_isBuilt)
                throw new InvalidOperationException("Cannot add attributes after the distinguished name is built.");
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));

            _attributes.Add($"{key}={EscapeValue(value)}");
        }

        /// <summary>
        /// Adds a common name to the distinguished name.
        /// </summary>
        /// <param name="commonName">The common name to add.</param>
        public void AddCommonName(string commonName) => AddAttribute("CN", commonName);

        /// <summary>
        /// Adds an organization name to the distinguished name.
        /// </summary>
        /// <param name="organizationName">The organization name to add.</param>
        public void AddOrganizationName(string organizationName) => AddAttribute("O", organizationName);

        /// <summary>
        /// Adds an organizational unit name to the distinguished name.
        /// </summary>
        /// <param name="organizationalUnitName">The organizational unit name to add.</param>
        public void AddOrganizationalUnitName(string organizationalUnitName) => AddAttribute("OU", organizationalUnitName);

        /// <summary>
        /// Adds a country code to the distinguished name.
        /// </summary>
        /// <param name="countryCode">The country code to add.</param>
        public void AddCountryCode(string countryCode) => AddAttribute("C", ValidateCountryCode(countryCode));

        /// <summary>
        /// Adds a locality name to the distinguished name.
        /// </summary>
        /// <param name="localityName">The locality name to add.</param>
        public void AddLocalityName(string localityName) => AddAttribute("L", localityName);

        /// <summary>
        /// Adds a state or province name to the distinguished name.
        /// </summary>
        /// <param name="stateOrProvinceName">The state or province name to add.</param>
        public void AddStateOrProvinceName(string stateOrProvinceName) => AddAttribute("ST", stateOrProvinceName);

        /// <summary>
        /// Adds a country or region to the distinguished name.
        /// </summary>
        /// <param name="countryOrRegion">The country or region to add.</param>
        public void AddCountryOrRegion(string countryOrRegion) => AddAttribute("C", countryOrRegion);

        /// <summary>
        /// Adds an email address to the distinguished name.
        /// </summary>
        /// <param name="emailAddress">The email address to add.</param>
        public void AddEmailAddress(string emailAddress) => AddAttribute("E", ValidateEmailAddress(emailAddress));


        /// <summary>
        /// Builds and returns an immutable distinguished name object.
        /// </summary>
        /// <returns>The built distinguished name as an immutable object.</returns>
        public DistinguishedName Build()
        {
            if (_isBuilt)
                throw new InvalidOperationException("Distinguished name has already been built.");

            _isBuilt = true;
            return new DistinguishedName(string.Join(", ", _attributes));
        }

        private static string ValidateCountryCode(string countryCode)
        {
            if (countryCode.Length != 2 || !Regex.IsMatch(countryCode, "^[A-Za-z]{2}$"))
                throw new ArgumentException("Country code must be exactly two letters.", nameof(countryCode));
            return countryCode.ToUpper();
        }

        private static string ValidateEmailAddress(string emailAddress)
        {
            if (!Regex.IsMatch(emailAddress, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Invalid email address format.", nameof(emailAddress));
            return emailAddress;
        }

        private static string EscapeValue(string value)
        {
            return value.Replace("+", "\\+")
                        .Replace(",", "\\,")
                        .Replace(";", "\\;")
                        .Replace("<", "\\<")
                        .Replace(">", "\\>")
                        .Replace("\"", "\\\"")
                        .Replace("\\", "\\\\");
        }
    }
    /// <summary>
    /// Represents an immutable distinguished name constructed by <see cref="CommonDistinguishedNameBuilder"/>.
    /// </summary>
    public class DistinguishedName
    {
        /// <summary>
        /// Gets the distinguished name string.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the DistinguishedName class with a specified name.
        /// </summary>
        /// <param name="name">The distinguished name string.</param>
        public DistinguishedName(string name)
        {
            Name = name;
        }
    }

}
