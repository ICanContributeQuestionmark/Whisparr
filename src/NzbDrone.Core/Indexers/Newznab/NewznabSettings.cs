using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabSettingsValidator : AbstractValidator<NewznabSettings>
    {
        private static readonly string[] ApiKeyWhiteList =
        {
            "nzbs.org",
            "nzb.su",
            "dognzb.cr",
            "nzbplanet.net",
            "nzbid.org",
            "nzbndx.com",
            "nzbindex.in"
        };

        private static bool ShouldHaveApiKey(NewznabSettings settings)
        {
            return settings.BaseUrl != null && ApiKeyWhiteList.Any(c => settings.BaseUrl.ToLowerInvariant().Contains(c));
        }

        private static readonly Regex AdditionalParametersRegex = new Regex(@"(&.+?\=.+?)+", RegexOptions.Compiled);

        public NewznabSettingsValidator()
        {
            RuleFor(c => c).Custom((c, context) =>
            {
                if (c.Categories.Empty())
                {
                    context.AddFailure("'Categories' must be provided");
                }
            });

            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ApiPath).ValidUrlBase("/api");
            RuleFor(c => c.ApiKey).NotEmpty().When(ShouldHaveApiKey);
            RuleFor(c => c.AdditionalParameters).Matches(AdditionalParametersRegex)
                                                .When(c => !c.AdditionalParameters.IsNullOrWhiteSpace());
        }
    }

    public class NewznabSettings : IIndexerSettings
    {
        private static readonly NewznabSettingsValidator Validator = new NewznabSettingsValidator();

        public NewznabSettings()
        {
            ApiPath = "/api";
            Categories = new[] { 6000, 6010, 6020, 6030, 6040, 6045, 6050, 6070, 6080, 6090 };
        }

        [FieldDefinition(0, Label = "URL")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "API Path", HelpText = "Path to the api, usually /api", Advanced = true)]
        public string ApiPath { get; set; }

        [FieldDefinition(2, Label = "API Key", Privacy = PrivacyLevel.ApiKey)]
        public string ApiKey { get; set; }

        [FieldDefinition(3, Label = "Categories", Type = FieldType.Select, SelectOptionsProviderAction = "newznabCategories", HelpText = "Drop down list, leave blank to disable standard/daily shows")]
        public IEnumerable<int> Categories { get; set; }

        [FieldDefinition(6, Label = "Additional Parameters", HelpText = "Additional Newznab parameters", Advanced = true)]
        public string AdditionalParameters { get; set; }

        // Field 7 is used by TorznabSettings MinimumSeeders
        // If you need to add another field here, update TorznabSettings as well and this comment

        public virtual NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
