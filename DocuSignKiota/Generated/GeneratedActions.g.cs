#nullable enable
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Serialization.Json;
using OutSystems.ExternalLibraries.SDK;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DocuSignKiota.Generated;

internal static class GeneratedModelMapper
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(), new JsonStringOrRawJsonConverter() }
    };

    private sealed class JsonStringOrRawJsonConverter : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }

            using var document = JsonDocument.ParseValue(ref reader);
            return document.RootElement.ValueKind switch
            {
                JsonValueKind.Object or JsonValueKind.Array => document.RootElement.GetRawText(),
                JsonValueKind.Null or JsonValueKind.Undefined => null,
                _ => document.RootElement.ToString()
            };
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }

    public static TDestination? Convert<TDestination>(object? source)
    {
        if (source is null)
        {
            return default;
        }

        var json = JsonSerializer.Serialize(source, SerializerOptions);
        return JsonSerializer.Deserialize<TDestination>(json, SerializerOptions);
    }

    public static byte[] ReadAllBytes(Stream? source)
    {
        if (source is null)
        {
            return Array.Empty<byte>();
        }

        using var buffer = new MemoryStream();
        source.CopyTo(buffer);
        return buffer.ToArray();
    }

    public static TDestination ParseJson<TDestination>(string source, ParsableFactory<TDestination> factory) where TDestination : IParsable
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(string.IsNullOrWhiteSpace(source) ? "{}" : source));
        var parseNode = ParseNodeFactoryRegistry.DefaultInstance.GetRootParseNodeAsync("application/json", stream).GetAwaiter().GetResult();
        return parseNode.GetObjectValue(factory)!;
    }

    public static TEnum? ParseEnum<TEnum>(string? source) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return null;
        }

        foreach (var value in Enum.GetValues<TEnum>())
        {
            var member = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
            var enumMember = member?.GetCustomAttribute<EnumMemberAttribute>();
            if (string.Equals(enumMember?.Value, source, StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }
        }

        var normalized = NormalizeEnumToken(source);
        return Enum.TryParse<TEnum>(normalized, true, out var parsed) ? parsed : null;
    }

    public static TEnum[] ParseEnumArray<TEnum>(IEnumerable<string>? source) where TEnum : struct, Enum
    {
        if (source is null)
        {
            return Array.Empty<TEnum>();
        }

        return source
            .Select(ParseEnum<TEnum>)
            .Where(item => item.HasValue)
            .Select(item => item!.Value)
            .ToArray();
    }

    public static Microsoft.Kiota.Abstractions.Date ToKiotaDate(DateTime source)
    {
        return new Microsoft.Kiota.Abstractions.Date(source.Year, source.Month, source.Day);
    }

    public static string SerializeObject(object? source)
    {
        if (source is null)
        {
            return string.Empty;
        }

        if (source is IAdditionalDataHolder additionalDataHolder && HasNoDeclaredPayload(source.GetType()))
        {
            return JsonSerializer.Serialize(additionalDataHolder.AdditionalData, SerializerOptions);
        }

        return JsonSerializer.Serialize(source, SerializerOptions);
    }

    private static bool HasNoDeclaredPayload(Type type)
    {
        return !type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Any(property => !string.Equals(property.Name, nameof(IAdditionalDataHolder.AdditionalData), StringComparison.Ordinal));
    }

    private static string NormalizeEnumToken(string source)
    {
        var builder = new StringBuilder(source.Length);
        var capitalizeNext = true;
        foreach (var character in source)
        {
            if (!char.IsLetterOrDigit(character))
            {
                capitalizeNext = true;
                continue;
            }

            builder.Append(capitalizeNext ? char.ToUpperInvariant(character) : character);
            capitalizeNext = false;
        }

        return builder.Length == 0 ? source : builder.ToString();
    }

}

public class DocuSignActionsGenerated : IDocuSignActionsGenerated
{
    private readonly DocuSignKiota.Client.DocuSignClient _client;

    public DocuSignActionsGenerated()
    {
        var requestAdapter = new HttpClientRequestAdapter(
            new AnonymousAuthenticationProvider(),
            new JsonParseNodeFactory(),
            new JsonSerializationWriterFactory(),
            new HttpClient(),
            null);

        _client = new DocuSignKiota.Client.DocuSignClient(requestAdapter);
    }

    public ServiceInformation ServiceInformationGetServiceInformation()
    {
        var requestBuilder = _client.Service_information;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ServiceInformation>(response)!;
    }

    public ResourceInformation ServiceInformationGetResourceInformation()
    {
        var requestBuilder = _client.V21;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ResourceInformation>(response)!;
    }

    public NewAccountSummary AccountsPostAccounts(NewAccountDefinition requestBody)
    {
        var requestBuilder = _client.V21.Accounts;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.NewAccountDefinition>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<NewAccountSummary>(response)!;
    }

    public AccountInformation AccountsGetAccount(string accountId, string includeAccountSettings = "", bool includeIncludeAccountSettings = false, string includeTrialEligibility = "", bool includeIncludeTrialEligibility = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeIncludeAccountSettings)
            {
                config.QueryParameters.IncludeAccountSettings = includeAccountSettings;
            }
            if (includeIncludeTrialEligibility)
            {
                config.QueryParameters.IncludeTrialEligibility = includeTrialEligibility;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountInformation>(response)!;
    }

    public byte[] AccountsDeleteAccount(string accountId, string redactUserData = "", bool includeRedactUserData = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public BillingChargeResponse BillingChargesGetAccountBillingCharges(string accountId, string includeCharges = "", bool includeIncludeCharges = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Billing_charges;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeIncludeCharges)
            {
                config.QueryParameters.IncludeCharges = includeCharges;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BillingChargeResponse>(response)!;
    }

    public BillingInvoicesResponse BillingInvoicesGetBillingInvoices(string accountId, string fromDate = "", bool includeFromDate = false, string toDate = "", bool includeToDate = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Billing_invoices;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeFromDate)
            {
                config.QueryParameters.FromDate = fromDate;
            }
            if (includeToDate)
            {
                config.QueryParameters.ToDate = toDate;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BillingInvoicesResponse>(response)!;
    }

    public BillingInvoice BillingInvoicesGetBillingInvoice(string accountId, string invoiceId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Billing_invoices[invoiceId];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BillingInvoice>(response)!;
    }

    public BillingInvoicesSummary BillingInvoicesGetBillingInvoicesPastDue(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Billing_invoices_past_due;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BillingInvoicesSummary>(response)!;
    }

    public BillingPaymentsResponse BillingPaymentsGetPaymentList(string accountId, string fromDate = "", bool includeFromDate = false, string toDate = "", bool includeToDate = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Billing_payments;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeFromDate)
            {
                config.QueryParameters.FromDate = fromDate;
            }
            if (includeToDate)
            {
                config.QueryParameters.ToDate = toDate;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BillingPaymentsResponse>(response)!;
    }

    public BillingPaymentResponse BillingPaymentsPostPayment(string accountId, BillingPaymentRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Billing_payments;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.BillingPaymentRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BillingPaymentResponse>(response)!;
    }

    public BillingPaymentItem BillingPaymentsGetPayment(string accountId, string paymentId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Billing_payments[paymentId];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BillingPaymentItem>(response)!;
    }

    public AccountBillingPlanResponse BillingPlanGetBillingPlan(string accountId, string includeCreditCardInformation = "", bool includeIncludeCreditCardInformation = false, string includeDowngradeInformation = "", bool includeIncludeDowngradeInformation = false, string includeMetadata = "", bool includeIncludeMetadata = false, string includeSuccessorPlans = "", bool includeIncludeSuccessorPlans = false, string includeTaxExemptId = "", bool includeIncludeTaxExemptId = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Billing_plan;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeIncludeCreditCardInformation)
            {
                config.QueryParameters.IncludeCreditCardInformation = includeCreditCardInformation;
            }
            if (includeIncludeDowngradeInformation)
            {
                config.QueryParameters.IncludeDowngradeInformation = includeDowngradeInformation;
            }
            if (includeIncludeMetadata)
            {
                config.QueryParameters.IncludeMetadata = includeMetadata;
            }
            if (includeIncludeSuccessorPlans)
            {
                config.QueryParameters.IncludeSuccessorPlans = includeSuccessorPlans;
            }
            if (includeIncludeTaxExemptId)
            {
                config.QueryParameters.IncludeTaxExemptId = includeTaxExemptId;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountBillingPlanResponse>(response)!;
    }

    public BillingPlanUpdateResponse BillingPlanPutBillingPlan(string accountId, BillingPlanInformation requestBody, string previewBillingPlan = "", bool includePreviewBillingPlan = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Billing_plan;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.BillingPlanInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BillingPlanUpdateResponse>(response)!;
    }

    public CreditCardInformation BillingPlanGetCreditCardInfo(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Billing_plan.Credit_card;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<CreditCardInformation>(response)!;
    }

    public DowngradRequestBillingInfoResponse BillingPlanGetDowngradeRequestBillingInfo(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Billing_plan.Downgrade;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DowngradRequestBillingInfoResponse>(response)!;
    }

    public DowngradePlanUpdateResponse BillingPlanPutDowngradeAccountBillingPlan(string accountId, DowngradeBillingPlanInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Billing_plan.Downgrade;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.DowngradeBillingPlanInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DowngradePlanUpdateResponse>(response)!;
    }

    public byte[] PurchasedEnvelopesPutPurchasedEnvelopes(string accountId, PurchasedEnvelopesInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Billing_plan.Purchased_envelopes;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.PurchasedEnvelopesInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public AccountBrands BrandsGetBrands(string accountId, string excludeDistributorBrand = "", bool includeExcludeDistributorBrand = false, string includeLogos = "", bool includeIncludeLogos = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Brands;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeExcludeDistributorBrand)
            {
                config.QueryParameters.ExcludeDistributorBrand = excludeDistributorBrand;
            }
            if (includeIncludeLogos)
            {
                config.QueryParameters.IncludeLogos = includeLogos;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountBrands>(response)!;
    }

    public AccountBrands BrandsPostBrands(string accountId, Brand requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Brands;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.Brand>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountBrands>(response)!;
    }

    public AccountBrands BrandsDeleteBrands(string accountId, BrandsRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Brands;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.BrandsRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountBrands>(response)!;
    }

    public Brand BrandGetBrand(string accountId, string brandId, string includeExternalReferences = "", bool includeIncludeExternalReferences = false, string includeLogos = "", bool includeIncludeLogos = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Brands[brandId];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeIncludeExternalReferences)
            {
                config.QueryParameters.IncludeExternalReferences = includeExternalReferences;
            }
            if (includeIncludeLogos)
            {
                config.QueryParameters.IncludeLogos = includeLogos;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Brand>(response)!;
    }

    public Brand BrandPutBrand(string accountId, string brandId, Brand requestBody, string replaceBrand = "", bool includeReplaceBrand = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Brands[brandId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.Brand>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Brand>(response)!;
    }

    public byte[] BrandDeleteBrand(string accountId, string brandId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Brands[brandId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public void BrandExportGetBrandExportFile(string accountId, string brandId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Brands[brandId].File;
        requestBuilder.GetAsync().GetAwaiter().GetResult();
    }

    public byte[] BrandLogoGetBrandLogo(string accountId, string brandId, string logoType)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Brands[brandId].Logos[logoType];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public byte[] BrandLogoPutBrandLogo(string accountId, string brandId, string logoType, byte[] requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Brands[brandId].Logos[logoType];
        var response = requestBuilder.PutAsync(new MemoryStream(requestBody ?? Array.Empty<byte>())).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public byte[] BrandLogoDeleteBrandLogo(string accountId, string brandId, string logoType)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Brands[brandId].Logos[logoType];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public BrandResourcesList BrandResourcesGetBrandResourcesList(string accountId, string brandId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Brands[brandId].Resources;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BrandResourcesList>(response)!;
    }

    public byte[] BrandResourcesGetBrandResources(string accountId, string brandId, string resourceContentType, string langcode = "", bool includeLangcode = false, string returnMaster = "", bool includeReturnMaster = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Brands[brandId].Resources[resourceContentType];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeLangcode)
            {
                config.QueryParameters.Langcode = langcode;
            }
            if (includeReturnMaster)
            {
                config.QueryParameters.ReturnMaster = returnMaster;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public BrandResources BrandResourcesPutBrandResources(string accountId, string brandId, string resourceContentType, BrandResourcesPutBrandResourcesRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Brands[brandId].Resources[resourceContentType];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<Microsoft.Kiota.Abstractions.MultipartBody>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BrandResources>(response)!;
    }

    public BulkSendBatchSummaries BulkSendV2BatchGetBulkSendBatches(string accountId, string batchIds = "", bool includeBatchIds = false, string count = "", bool includeCount = false, string fromDate = "", bool includeFromDate = false, string searchText = "", bool includeSearchText = false, string startPosition = "", bool includeStartPosition = false, string status = "", bool includeStatus = false, string toDate = "", bool includeToDate = false, string userId = "", bool includeUserId = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Bulk_send_batch;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeBatchIds)
            {
                config.QueryParameters.BatchIds = batchIds;
            }
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeFromDate)
            {
                config.QueryParameters.FromDate = fromDate;
            }
            if (includeSearchText)
            {
                config.QueryParameters.SearchText = searchText;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
            if (includeStatus)
            {
                config.QueryParameters.Status = status;
            }
            if (includeToDate)
            {
                config.QueryParameters.ToDate = toDate;
            }
            if (includeUserId)
            {
                config.QueryParameters.UserId = userId;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BulkSendBatchSummaries>(response)!;
    }

    public BulkSendBatchStatus BulkSendV2BatchGetBulkSendBatchStatus(string accountId, string bulkSendBatchId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Bulk_send_batch[bulkSendBatchId];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BulkSendBatchStatus>(response)!;
    }

    public BulkSendBatchStatus BulkSendV2BatchPutBulkSendBatchStatus(string accountId, string bulkSendBatchId, BulkSendBatchRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Bulk_send_batch[bulkSendBatchId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.BulkSendBatchRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BulkSendBatchStatus>(response)!;
    }

    public BulkSendBatchStatus BulkSendV2BatchPutBulkSendBatchAction(string accountId, string bulkAction, string bulkSendBatchId, BulkSendBatchActionRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Bulk_send_batch[bulkSendBatchId][bulkAction];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.BulkSendBatchActionRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BulkSendBatchStatus>(response)!;
    }

    public EnvelopesInformation BulkSendV2EnvelopesGetBulkSendBatchEnvelopes(string accountId, string bulkSendBatchId, string count = "", bool includeCount = false, string include = "", bool includeInclude = false, string order = "", bool includeOrder = false, string orderBy = "", bool includeOrderBy = false, string searchText = "", bool includeSearchText = false, string startPosition = "", bool includeStartPosition = false, string status = "", bool includeStatus = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Bulk_send_batch[bulkSendBatchId].Envelopes;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeInclude)
            {
                config.QueryParameters.Include = include;
            }
            if (includeOrder)
            {
                config.QueryParameters.Order = order;
            }
            if (includeOrderBy)
            {
                config.QueryParameters.OrderBy = orderBy;
            }
            if (includeSearchText)
            {
                config.QueryParameters.SearchText = searchText;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
            if (includeStatus)
            {
                config.QueryParameters.Status = status;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopesInformation>(response)!;
    }

    public BulkSendingListSummaries BulkSendV2CRUDGetBulkSendLists(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Bulk_send_lists;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BulkSendingListSummaries>(response)!;
    }

    public BulkSendingList BulkSendV2CRUDPostBulkSendList(string accountId, BulkSendingList requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Bulk_send_lists;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.BulkSendingList>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BulkSendingList>(response)!;
    }

    public BulkSendingList BulkSendV2CRUDGetBulkSendList(string accountId, string bulkSendListId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Bulk_send_lists[bulkSendListId];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BulkSendingList>(response)!;
    }

    public BulkSendingList BulkSendV2CRUDPutBulkSendList(string accountId, string bulkSendListId, BulkSendingList requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Bulk_send_lists[bulkSendListId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.BulkSendingList>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BulkSendingList>(response)!;
    }

    public BulkSendingListSummaries BulkSendV2CRUDDeleteBulkSendList(string accountId, string bulkSendListId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Bulk_send_lists[bulkSendListId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BulkSendingListSummaries>(response)!;
    }

    public BulkSendResponse BulkSendV2SendPostBulkSendRequest(string accountId, string bulkSendListId, BulkSendRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Bulk_send_lists[bulkSendListId].Send;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.BulkSendRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BulkSendResponse>(response)!;
    }

    public BulkSendTestResponse BulkSendV2TestPostBulkSendTestRequest(string accountId, string bulkSendListId, BulkSendRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Bulk_send_lists[bulkSendListId].Test;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.BulkSendRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BulkSendTestResponse>(response)!;
    }

    public CaptiveRecipientInformation CaptiveRecipientsDeleteCaptiveRecipientsPart(string accountId, string recipientPart, CaptiveRecipientInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Captive_recipients[recipientPart];
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.CaptiveRecipientInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<CaptiveRecipientInformation>(response)!;
    }

    public ChunkedUploadResponse ChunkedUploadsPostChunkedUploads(string accountId, ChunkedUploadRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Chunked_uploads;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.ChunkedUploadRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ChunkedUploadResponse>(response)!;
    }

    public ChunkedUploadResponse ChunkedUploadsGetChunkedUpload(string accountId, string chunkedUploadId, string include = "", bool includeInclude = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Chunked_uploads[chunkedUploadId];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeInclude)
            {
                config.QueryParameters.Include = include;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ChunkedUploadResponse>(response)!;
    }

    public ChunkedUploadResponse ChunkedUploadsPutChunkedUploads(string accountId, string chunkedUploadId, string action = "", bool includeAction = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Chunked_uploads[chunkedUploadId];
        var response = requestBuilder.PutAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ChunkedUploadResponse>(response)!;
    }

    public ChunkedUploadResponse ChunkedUploadsDeleteChunkedUpload(string accountId, string chunkedUploadId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Chunked_uploads[chunkedUploadId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ChunkedUploadResponse>(response)!;
    }

    public ChunkedUploadResponse ChunkedUploadsPutChunkedUploadPart(string accountId, string chunkedUploadId, string chunkedUploadPartSeq, ChunkedUploadRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Chunked_uploads[chunkedUploadId][chunkedUploadPartSeq];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.ChunkedUploadRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ChunkedUploadResponse>(response)!;
    }

    public ConnectConfigResults ConnectGetConnectConfigs(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Connect;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ConnectConfigResults>(response)!;
    }

    public ConnectCustomConfiguration ConnectPutConnectConfiguration(string accountId, ConnectCustomConfiguration requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Connect;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.ConnectCustomConfiguration>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ConnectCustomConfiguration>(response)!;
    }

    public ConnectCustomConfiguration ConnectPostConnectConfiguration(string accountId, ConnectCustomConfiguration requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Connect;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.ConnectCustomConfiguration>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ConnectCustomConfiguration>(response)!;
    }

    public ConnectConfigResults ConnectGetConnectConfig(string accountId, string connectId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Connect[connectId];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ConnectConfigResults>(response)!;
    }

    public byte[] ConnectDeleteConnectConfig(string accountId, string connectId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Connect[connectId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public IntegratedConnectUserInfoList ConnectGetConnectAllUsers(string accountId, string connectId, string count = "", bool includeCount = false, string domainUsersOnly = "", bool includeDomainUsersOnly = false, string emailSubstring = "", bool includeEmailSubstring = false, string startPosition = "", bool includeStartPosition = false, string status = "", bool includeStatus = false, string userNameSubstring = "", bool includeUserNameSubstring = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Connect[connectId].All.Users;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeDomainUsersOnly)
            {
                config.QueryParameters.DomainUsersOnly = domainUsersOnly;
            }
            if (includeEmailSubstring)
            {
                config.QueryParameters.EmailSubstring = emailSubstring;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
            if (includeStatus)
            {
                config.QueryParameters.Status = status;
            }
            if (includeUserNameSubstring)
            {
                config.QueryParameters.UserNameSubstring = userNameSubstring;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<IntegratedConnectUserInfoList>(response)!;
    }

    public IntegratedUserInfoList ConnectGetConnectUsers(string accountId, string connectId, string count = "", bool includeCount = false, string emailSubstring = "", bool includeEmailSubstring = false, string listIncludedUsers = "", bool includeListIncludedUsers = false, string startPosition = "", bool includeStartPosition = false, string status = "", bool includeStatus = false, string userNameSubstring = "", bool includeUserNameSubstring = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Connect[connectId].Users;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeEmailSubstring)
            {
                config.QueryParameters.EmailSubstring = emailSubstring;
            }
            if (includeListIncludedUsers)
            {
                config.QueryParameters.ListIncludedUsers = listIncludedUsers;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
            if (includeStatus)
            {
                config.QueryParameters.Status = status;
            }
            if (includeUserNameSubstring)
            {
                config.QueryParameters.UserNameSubstring = userNameSubstring;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<IntegratedUserInfoList>(response)!;
    }

    public ConnectFailureResults ConnectPublishPutConnectRetryByEnvelope(string accountId, string envelopeId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Connect.Envelopes[envelopeId].Retry_queue;
        var response = requestBuilder.PutAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ConnectFailureResults>(response)!;
    }

    public EnvelopePublishTransaction HistoricalEnvelopePublishPostHistoricalEn_2a991cb6(string accountId, ConnectHistoricalEnvelopeRepublish requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Connect.Envelopes.Publish.Historical;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.ConnectHistoricalEnvelopeRepublish>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopePublishTransaction>(response)!;
    }

    public ConnectFailureResults ConnectPublishPutConnectRetry(string accountId, ConnectFailureFilter requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Connect.Envelopes.Retry_queue;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.ConnectFailureFilter>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ConnectFailureResults>(response)!;
    }

    public ConnectLogs ConnectFailuresGetConnectLogs(string accountId, string fromDate = "", bool includeFromDate = false, string toDate = "", bool includeToDate = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Connect.Failures;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeFromDate)
            {
                config.QueryParameters.FromDate = fromDate;
            }
            if (includeToDate)
            {
                config.QueryParameters.ToDate = toDate;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ConnectLogs>(response)!;
    }

    public string ConnectFailuresDeleteConnectFailureLog(string accountId, string failureId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Connect.Failures[failureId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.SerializeObject(response);
    }

    public ConnectLogs ConnectLogGetConnectLogs(string accountId, string fromDate = "", bool includeFromDate = false, string toDate = "", bool includeToDate = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Connect.Logs;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeFromDate)
            {
                config.QueryParameters.FromDate = fromDate;
            }
            if (includeToDate)
            {
                config.QueryParameters.ToDate = toDate;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ConnectLogs>(response)!;
    }

    public byte[] ConnectLogDeleteConnectLogs(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Connect.Logs;
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public ConnectLog ConnectLogGetConnectLog(string accountId, string logId, string additionalInfo = "", bool includeAdditionalInfo = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Connect.Logs[logId];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeAdditionalInfo)
            {
                config.QueryParameters.AdditionalInfo = additionalInfo;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ConnectLog>(response)!;
    }

    public byte[] ConnectLogDeleteConnectLog(string accountId, string logId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Connect.Logs[logId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public ConnectOAuthConfig ConnectOAuthConfigGetConnectOAuthConfig(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Connect.Oauth;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ConnectOAuthConfig>(response)!;
    }

    public ConnectOAuthConfig ConnectOAuthConfigPutConnectOAuthConfig(string accountId, ConnectOAuthConfig requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Connect.Oauth;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.ConnectOAuthConfig>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ConnectOAuthConfig>(response)!;
    }

    public ConnectOAuthConfig ConnectOAuthConfigPostConnectOAuthConfig(string accountId, ConnectOAuthConfig requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Connect.Oauth;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.ConnectOAuthConfig>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ConnectOAuthConfig>(response)!;
    }

    public byte[] ConnectOAuthConfigDeleteConnectOAuthConfig(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Connect.Oauth;
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public AccountConsumerDisclosures ConsumerDisclosureGetConsumerDisclosure(string accountId, string langCode = "", bool includeLangCode = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Consumer_disclosure;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeLangCode)
            {
                config.QueryParameters.LangCode = langCode;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountConsumerDisclosures>(response)!;
    }

    public AccountConsumerDisclosures ConsumerDisclosureGetConsumerDisclosureLangCode(string accountId, string langCode)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Consumer_disclosure[langCode];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountConsumerDisclosures>(response)!;
    }

    public ConsumerDisclosure ConsumerDisclosurePutConsumerDisclosure(string accountId, string langCode, ConsumerDisclosure requestBody, string includeMetadata = "", bool includeIncludeMetadata = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Consumer_disclosure[langCode];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.ConsumerDisclosure>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ConsumerDisclosure>(response)!;
    }

    public ContactUpdateResponse ContactsPutContacts(string accountId, ContactModRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Contacts;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.ContactModRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ContactUpdateResponse>(response)!;
    }

    public ContactUpdateResponse ContactsPostContacts(string accountId, ContactModRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Contacts;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.ContactModRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ContactUpdateResponse>(response)!;
    }

    public ContactUpdateResponse ContactsDeleteContacts(string accountId, ContactModRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Contacts;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.ContactModRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ContactUpdateResponse>(response)!;
    }

    public ContactGetResponse ContactsGetContactById(string accountId, string contactId, string cloudProvider = "", bool includeCloudProvider = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Contacts[contactId];
        var response = requestBuilder.GetAsContactGetResponseAsync(config =>
        {
            if (includeCloudProvider)
            {
                config.QueryParameters.CloudProvider = cloudProvider;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ContactGetResponse>(response)!;
    }

    public ContactUpdateResponse ContactsDeleteContactWithId(string accountId, string contactId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Contacts[contactId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ContactUpdateResponse>(response)!;
    }

    public AccountCustomFields AccountCustomFieldsGetAccountCustomFields(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Custom_fields;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountCustomFields>(response)!;
    }

    public AccountCustomFields AccountCustomFieldsPostAccountCustomFields(string accountId, CustomField requestBody, string applyToTemplates = "", bool includeApplyToTemplates = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Custom_fields;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.CustomField>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountCustomFields>(response)!;
    }

    public AccountCustomFields AccountCustomFieldsPutAccountCustomFields(string accountId, string customFieldId, CustomField requestBody, string applyToTemplates = "", bool includeApplyToTemplates = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Custom_fields[customFieldId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.CustomField>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountCustomFields>(response)!;
    }

    public byte[] AccountCustomFieldsDeleteAccountCustomFields(string accountId, string customFieldId, string applyToTemplates = "", bool includeApplyToTemplates = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Custom_fields[customFieldId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public EnvelopesInformation EnvelopesGetEnvelopes(string accountId, string acStatus = "", bool includeAcStatus = false, string block = "", bool includeBlock = false, string cdseMode = "", bool includeCdseMode = false, string continuationToken = "", bool includeContinuationToken = false, string count = "", bool includeCount = false, string customField = "", bool includeCustomField = false, string email = "", bool includeEmail = false, string envelopeIds = "", bool includeEnvelopeIds = false, string exclude = "", bool includeExclude = false, string folderIds = "", bool includeFolderIds = false, string folderTypes = "", bool includeFolderTypes = false, string fromDate = "", bool includeFromDate = false, string fromToStatus = "", bool includeFromToStatus = false, string include = "", bool includeInclude = false, string includePurgeInformation = "", bool includeIncludePurgeInformation = false, string intersectingFolderIds = "", bool includeIntersectingFolderIds = false, string lastQueriedDate = "", bool includeLastQueriedDate = false, string order = "", bool includeOrder = false, string orderBy = "", bool includeOrderBy = false, string powerformids = "", bool includePowerformids = false, string queryBudget = "", bool includeQueryBudget = false, string requesterDateFormat = "", bool includeRequesterDateFormat = false, string searchMode = "", bool includeSearchMode = false, string searchText = "", bool includeSearchText = false, string startPosition = "", bool includeStartPosition = false, string status = "", bool includeStatus = false, string toDate = "", bool includeToDate = false, string transactionIds = "", bool includeTransactionIds = false, string userFilter = "", bool includeUserFilter = false, string userId = "", bool includeUserId = false, string userName = "", bool includeUserName = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeAcStatus)
            {
                config.QueryParameters.AcStatus = acStatus;
            }
            if (includeBlock)
            {
                config.QueryParameters.Block = block;
            }
            if (includeCdseMode)
            {
                config.QueryParameters.CdseMode = cdseMode;
            }
            if (includeContinuationToken)
            {
                config.QueryParameters.ContinuationToken = continuationToken;
            }
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeCustomField)
            {
                config.QueryParameters.CustomField = customField;
            }
            if (includeEmail)
            {
                config.QueryParameters.Email = email;
            }
            if (includeEnvelopeIds)
            {
                config.QueryParameters.EnvelopeIds = envelopeIds;
            }
            if (includeExclude)
            {
                config.QueryParameters.Exclude = exclude;
            }
            if (includeFolderIds)
            {
                config.QueryParameters.FolderIds = folderIds;
            }
            if (includeFolderTypes)
            {
                config.QueryParameters.FolderTypes = folderTypes;
            }
            if (includeFromDate)
            {
                config.QueryParameters.FromDate = fromDate;
            }
            if (includeFromToStatus)
            {
                config.QueryParameters.FromToStatus = fromToStatus;
            }
            if (includeInclude)
            {
                config.QueryParameters.Include = include;
            }
            if (includeIncludePurgeInformation)
            {
                config.QueryParameters.IncludePurgeInformation = includePurgeInformation;
            }
            if (includeIntersectingFolderIds)
            {
                config.QueryParameters.IntersectingFolderIds = intersectingFolderIds;
            }
            if (includeLastQueriedDate)
            {
                config.QueryParameters.LastQueriedDate = lastQueriedDate;
            }
            if (includeOrder)
            {
                config.QueryParameters.Order = order;
            }
            if (includeOrderBy)
            {
                config.QueryParameters.OrderBy = orderBy;
            }
            if (includePowerformids)
            {
                config.QueryParameters.Powerformids = powerformids;
            }
            if (includeQueryBudget)
            {
                config.QueryParameters.QueryBudget = queryBudget;
            }
            if (includeRequesterDateFormat)
            {
                config.QueryParameters.RequesterDateFormat = requesterDateFormat;
            }
            if (includeSearchMode)
            {
                config.QueryParameters.SearchMode = searchMode;
            }
            if (includeSearchText)
            {
                config.QueryParameters.SearchText = searchText;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
            if (includeStatus)
            {
                config.QueryParameters.Status = status;
            }
            if (includeToDate)
            {
                config.QueryParameters.ToDate = toDate;
            }
            if (includeTransactionIds)
            {
                config.QueryParameters.TransactionIds = transactionIds;
            }
            if (includeUserFilter)
            {
                config.QueryParameters.UserFilter = userFilter;
            }
            if (includeUserId)
            {
                config.QueryParameters.UserId = userId;
            }
            if (includeUserName)
            {
                config.QueryParameters.UserName = userName;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopesInformation>(response)!;
    }

    public EnvelopeSummary EnvelopesPostEnvelopes(string accountId, EnvelopeDefinition requestBody, string cdseMode = "", bool includeCdseMode = false, string changeRoutingOrder = "", bool includeChangeRoutingOrder = false, string completedDocumentsOnly = "", bool includeCompletedDocumentsOnly = false, string mergeRolesOnDraft = "", bool includeMergeRolesOnDraft = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeDefinition>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeSummary>(response)!;
    }

    public Envelope EnvelopesGetEnvelope(string accountId, string envelopeId, string advancedUpdate = "", bool includeAdvancedUpdate = false, string include = "", bool includeInclude = false, string includeAnchorTabLocations = "", bool includeIncludeAnchorTabLocations = false, string userId = "", bool includeUserId = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeAdvancedUpdate)
            {
                config.QueryParameters.AdvancedUpdate = advancedUpdate;
            }
            if (includeInclude)
            {
                config.QueryParameters.Include = include;
            }
            if (includeIncludeAnchorTabLocations)
            {
                config.QueryParameters.IncludeAnchorTabLocations = includeAnchorTabLocations;
            }
            if (includeUserId)
            {
                config.QueryParameters.UserId = userId;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Envelope>(response)!;
    }

    public EnvelopeUpdateSummary EnvelopesPutEnvelope(string accountId, string envelopeId, Envelope requestBody, string advancedUpdate = "", bool includeAdvancedUpdate = false, string recycleOnVoid = "", bool includeRecycleOnVoid = false, string resendEnvelope = "", bool includeResendEnvelope = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.Envelope>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeUpdateSummary>(response)!;
    }

    public EnvelopeAttachmentsResult AttachmentsGetAttachments(string accountId, string envelopeId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Attachments;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeAttachmentsResult>(response)!;
    }

    public EnvelopeAttachmentsResult AttachmentsPutAttachments(string accountId, string envelopeId, EnvelopeAttachmentsRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Attachments;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeAttachmentsRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeAttachmentsResult>(response)!;
    }

    public EnvelopeAttachmentsResult AttachmentsDeleteAttachments(string accountId, string envelopeId, EnvelopeAttachmentsRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Attachments;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeAttachmentsRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeAttachmentsResult>(response)!;
    }

    public byte[] AttachmentsGetAttachment(string accountId, string attachmentId, string envelopeId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Attachments[attachmentId];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public EnvelopeAttachmentsResult AttachmentsPutAttachment(string accountId, string attachmentId, string envelopeId, Attachment requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Attachments[attachmentId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.Attachment>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeAttachmentsResult>(response)!;
    }

    public EnvelopeAuditEventResponse AuditEventsGetAuditEvents(string accountId, string envelopeId, string locale = "", bool includeLocale = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Audit_events;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeLocale)
            {
                config.QueryParameters.Locale = locale;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeAuditEventResponse>(response)!;
    }

    public byte[] CommentsGetCommentsTranscript(string accountId, string envelopeId, string encoding = "", bool includeEncoding = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Comments.Transcript;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeEncoding)
            {
                config.QueryParameters.Encoding = encoding;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public CustomFieldsEnvelope CustomFieldsGetCustomFields(string accountId, string envelopeId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Custom_fields;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<CustomFieldsEnvelope>(response)!;
    }

    public EnvelopeCustomFields CustomFieldsPutCustomFields(string accountId, string envelopeId, EnvelopeCustomFields requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Custom_fields;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeCustomFields>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeCustomFields>(response)!;
    }

    public EnvelopeCustomFields CustomFieldsPostCustomFields(string accountId, string envelopeId, EnvelopeCustomFields requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Custom_fields;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeCustomFields>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeCustomFields>(response)!;
    }

    public EnvelopeCustomFields CustomFieldsDeleteCustomFields(string accountId, string envelopeId, EnvelopeCustomFields requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Custom_fields;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeCustomFields>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeCustomFields>(response)!;
    }

    public DocGenFormFieldResponse DocGenFormFieldsGetEnvelopeDocGenFormFields(string accountId, string envelopeId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].DocGenFormFields;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DocGenFormFieldResponse>(response)!;
    }

    public DocGenFormFieldResponse DocGenFormFieldsPutEnvelopeDocGenFormFields(string accountId, string envelopeId, DocGenFormFieldRequest requestBody, string updateDocgenFormfieldsOnly = "", bool includeUpdateDocgenFormfieldsOnly = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].DocGenFormFields;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.DocGenFormFieldRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DocGenFormFieldResponse>(response)!;
    }

    public EnvelopeDocumentsResult DocumentsGetDocuments(string accountId, string envelopeId, string documentsByUserid = "", bool includeDocumentsByUserid = false, string includeAgreementType = "", bool includeIncludeAgreementType = false, string includeDocgenFormfields = "", bool includeIncludeDocgenFormfields = false, string includeMetadata = "", bool includeIncludeMetadata = false, string includeTabs = "", bool includeIncludeTabs = false, string recipientId = "", bool includeRecipientId = false, string sharedUserId = "", bool includeSharedUserId = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeDocumentsByUserid)
            {
                config.QueryParameters.DocumentsByUserid = documentsByUserid;
            }
            if (includeIncludeAgreementType)
            {
                config.QueryParameters.IncludeAgreementType = includeAgreementType;
            }
            if (includeIncludeDocgenFormfields)
            {
                config.QueryParameters.IncludeDocgenFormfields = includeDocgenFormfields;
            }
            if (includeIncludeMetadata)
            {
                config.QueryParameters.IncludeMetadata = includeMetadata;
            }
            if (includeIncludeTabs)
            {
                config.QueryParameters.IncludeTabs = includeTabs;
            }
            if (includeRecipientId)
            {
                config.QueryParameters.RecipientId = recipientId;
            }
            if (includeSharedUserId)
            {
                config.QueryParameters.SharedUserId = sharedUserId;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeDocumentsResult>(response)!;
    }

    public EnvelopeDocumentsResult DocumentsPutDocuments(string accountId, string envelopeId, EnvelopeDefinition requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeDefinition>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeDocumentsResult>(response)!;
    }

    public EnvelopeDocumentsResult DocumentsDeleteDocuments(string accountId, string envelopeId, EnvelopeDefinition requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeDefinition>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeDocumentsResult>(response)!;
    }

    public byte[] DocumentsGetDocument(string accountId, string documentId, string envelopeId, string certificate = "", bool includeCertificate = false, string documentsByUserid = "", bool includeDocumentsByUserid = false, string encoding = "", bool includeEncoding = false, string encrypt = "", bool includeEncrypt = false, string language = "", bool includeLanguage = false, string recipientId = "", bool includeRecipientId = false, string sharedUserId = "", bool includeSharedUserId = false, string showChanges = "", bool includeShowChanges = false, string watermark = "", bool includeWatermark = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents[documentId];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCertificate)
            {
                config.QueryParameters.Certificate = certificate;
            }
            if (includeDocumentsByUserid)
            {
                config.QueryParameters.DocumentsByUserid = documentsByUserid;
            }
            if (includeEncoding)
            {
                config.QueryParameters.Encoding = encoding;
            }
            if (includeEncrypt)
            {
                config.QueryParameters.Encrypt = encrypt;
            }
            if (includeLanguage)
            {
                config.QueryParameters.Language = language;
            }
            if (includeRecipientId)
            {
                config.QueryParameters.RecipientId = recipientId;
            }
            if (includeSharedUserId)
            {
                config.QueryParameters.SharedUserId = sharedUserId;
            }
            if (includeShowChanges)
            {
                config.QueryParameters.ShowChanges = showChanges;
            }
            if (includeWatermark)
            {
                config.QueryParameters.Watermark = watermark;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public EnvelopeDocument DocumentsPutDocument(string accountId, string documentId, string envelopeId, byte[] requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents[documentId];
        var response = requestBuilder.PutAsync(new MemoryStream(requestBody ?? Array.Empty<byte>())).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeDocument>(response)!;
    }

    public EnvelopeDocumentFields DocumentFieldsGetDocumentFields(string accountId, string documentId, string envelopeId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents[documentId].Fields;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeDocumentFields>(response)!;
    }

    public EnvelopeDocumentFields DocumentFieldsPutDocumentFields(string accountId, string documentId, string envelopeId, EnvelopeDocumentFields requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents[documentId].Fields;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeDocumentFields>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeDocumentFields>(response)!;
    }

    public EnvelopeDocumentFields DocumentFieldsPostDocumentFields(string accountId, string documentId, string envelopeId, EnvelopeDocumentFields requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents[documentId].Fields;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeDocumentFields>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeDocumentFields>(response)!;
    }

    public EnvelopeDocumentFields DocumentFieldsDeleteDocumentFields(string accountId, string documentId, string envelopeId, EnvelopeDocumentFields requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents[documentId].Fields;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeDocumentFields>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeDocumentFields>(response)!;
    }

    public DocumentHtmlDefinitionOriginals ResponsiveHtmlGetEnvelopeDocumentHtmlDefinitions(string accountId, string documentId, string envelopeId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents[documentId].Html_definitions;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DocumentHtmlDefinitionOriginals>(response)!;
    }

    public PageImages PagesGetPageImages(string accountId, string documentId, string envelopeId, string count = "", bool includeCount = false, string dpi = "", bool includeDpi = false, string maxHeight = "", bool includeMaxHeight = false, string maxWidth = "", bool includeMaxWidth = false, string nocache = "", bool includeNocache = false, string showChanges = "", bool includeShowChanges = false, string startPosition = "", bool includeStartPosition = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents[documentId].Pages;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeDpi)
            {
                config.QueryParameters.Dpi = dpi;
            }
            if (includeMaxHeight)
            {
                config.QueryParameters.MaxHeight = maxHeight;
            }
            if (includeMaxWidth)
            {
                config.QueryParameters.MaxWidth = maxWidth;
            }
            if (includeNocache)
            {
                config.QueryParameters.Nocache = nocache;
            }
            if (includeShowChanges)
            {
                config.QueryParameters.ShowChanges = showChanges;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<PageImages>(response)!;
    }

    public byte[] PagesDeletePage(string accountId, string documentId, string envelopeId, string pageNumber)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents[documentId].Pages[pageNumber];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public byte[] PagesGetPageImage(string accountId, string documentId, string envelopeId, string pageNumber, string dpi = "", bool includeDpi = false, string maxHeight = "", bool includeMaxHeight = false, string maxWidth = "", bool includeMaxWidth = false, string showChanges = "", bool includeShowChanges = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents[documentId].Pages[pageNumber].Page_image;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeDpi)
            {
                config.QueryParameters.Dpi = dpi;
            }
            if (includeMaxHeight)
            {
                config.QueryParameters.MaxHeight = maxHeight;
            }
            if (includeMaxWidth)
            {
                config.QueryParameters.MaxWidth = maxWidth;
            }
            if (includeShowChanges)
            {
                config.QueryParameters.ShowChanges = showChanges;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public byte[] PagesPutPageImage(string accountId, string documentId, string envelopeId, string pageNumber, PageRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents[documentId].Pages[pageNumber].Page_image;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.PageRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public EnvelopeDocumentTabs TabsGetPageTabs(string accountId, string documentId, string envelopeId, string pageNumber)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents[documentId].Pages[pageNumber].Tabs;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeDocumentTabs>(response)!;
    }

    public DocumentHtmlDefinitions ResponsiveHtmlPostDocumentResponsiveHtmlPreview(string accountId, string documentId, string envelopeId, DocumentHtmlDefinition requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents[documentId].Responsive_html_preview;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.DocumentHtmlDefinition>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DocumentHtmlDefinitions>(response)!;
    }

    public EnvelopeDocumentTabs TabsGetDocumentTabs(string accountId, string documentId, string envelopeId, string includeMetadata = "", bool includeIncludeMetadata = false, string pageNumbers = "", bool includePageNumbers = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents[documentId].Tabs;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeIncludeMetadata)
            {
                config.QueryParameters.IncludeMetadata = includeMetadata;
            }
            if (includePageNumbers)
            {
                config.QueryParameters.PageNumbers = pageNumbers;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeDocumentTabs>(response)!;
    }

    public Tabs TabsPutDocumentTabs(string accountId, string documentId, string envelopeId, Tabs requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents[documentId].Tabs;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.Tabs>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Tabs>(response)!;
    }

    public Tabs TabsPostDocumentTabs(string accountId, string documentId, string envelopeId, Tabs requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents[documentId].Tabs;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.Tabs>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Tabs>(response)!;
    }

    public Tabs TabsDeleteDocumentTabs(string accountId, string documentId, string envelopeId, Tabs requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents[documentId].Tabs;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.Tabs>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Tabs>(response)!;
    }

    public TemplateInformation TemplatesGetDocumentTemplates(string accountId, string documentId, string envelopeId, string include = "", bool includeInclude = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents[documentId].Templates;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeInclude)
            {
                config.QueryParameters.Include = include;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<TemplateInformation>(response)!;
    }

    public DocumentTemplateList TemplatesPostDocumentTemplates(string accountId, string documentId, string envelopeId, DocumentTemplateList requestBody, string preserveTemplateRecipient = "", bool includePreserveTemplateRecipient = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents[documentId].Templates;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.DocumentTemplateList>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DocumentTemplateList>(response)!;
    }

    public byte[] TemplatesDeleteDocumentTemplates(string accountId, string documentId, string envelopeId, string templateId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Documents[documentId].Templates[templateId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public EmailSettings EmailSettingsGetEmailSettings(string accountId, string envelopeId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Email_settings;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EmailSettings>(response)!;
    }

    public EmailSettings EmailSettingsPutEmailSettings(string accountId, string envelopeId, EmailSettings requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Email_settings;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EmailSettings>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EmailSettings>(response)!;
    }

    public EmailSettings EmailSettingsPostEmailSettings(string accountId, string envelopeId, EmailSettings requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Email_settings;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EmailSettings>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EmailSettings>(response)!;
    }

    public EmailSettings EmailSettingsDeleteEmailSettings(string accountId, string envelopeId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Email_settings;
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EmailSettings>(response)!;
    }

    public EnvelopeFormData FormDataGetFormData(string accountId, string envelopeId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Form_data;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeFormData>(response)!;
    }

    public DocumentHtmlDefinitionOriginals ResponsiveHtmlGetEnvelopeHtmlDefinitions(string accountId, string envelopeId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Html_definitions;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DocumentHtmlDefinitionOriginals>(response)!;
    }

    public EnvelopeLocks LockGetEnvelopeLock(string accountId, string envelopeId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Lock;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeLocks>(response)!;
    }

    public EnvelopeLocks LockPutEnvelopeLock(string accountId, string envelopeId, LockRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Lock;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.LockRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeLocks>(response)!;
    }

    public EnvelopeLocks LockPostEnvelopeLock(string accountId, string envelopeId, LockRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Lock;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.LockRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeLocks>(response)!;
    }

    public EnvelopeLocks LockDeleteEnvelopeLock(string accountId, string envelopeId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Lock;
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeLocks>(response)!;
    }

    public Notification NotificationGetEnvelopesEnvelopeIdNotification(string accountId, string envelopeId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Notification;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Notification>(response)!;
    }

    public Notification NotificationPutEnvelopesEnvelopeIdNotification(string accountId, string envelopeId, EnvelopeNotificationRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Notification;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeNotificationRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Notification>(response)!;
    }

    public EnvelopeRecipients RecipientsGetRecipients(string accountId, string envelopeId, string includeAnchorTabLocations = "", bool includeIncludeAnchorTabLocations = false, string includeExtended = "", bool includeIncludeExtended = false, string includeMetadata = "", bool includeIncludeMetadata = false, string includeTabs = "", bool includeIncludeTabs = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeIncludeAnchorTabLocations)
            {
                config.QueryParameters.IncludeAnchorTabLocations = includeAnchorTabLocations;
            }
            if (includeIncludeExtended)
            {
                config.QueryParameters.IncludeExtended = includeExtended;
            }
            if (includeIncludeMetadata)
            {
                config.QueryParameters.IncludeMetadata = includeMetadata;
            }
            if (includeIncludeTabs)
            {
                config.QueryParameters.IncludeTabs = includeTabs;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeRecipients>(response)!;
    }

    public RecipientsUpdateSummary RecipientsPutRecipients(string accountId, string envelopeId, EnvelopeRecipients requestBody, string combineSameOrderRecipients = "", bool includeCombineSameOrderRecipients = false, string offlineSigning = "", bool includeOfflineSigning = false, string resendEnvelope = "", bool includeResendEnvelope = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeRecipients>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<RecipientsUpdateSummary>(response)!;
    }

    public EnvelopeRecipients RecipientsPostRecipients(string accountId, string envelopeId, EnvelopeRecipients requestBody, string resendEnvelope = "", bool includeResendEnvelope = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeRecipients>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeRecipients>(response)!;
    }

    public EnvelopeRecipients RecipientsDeleteRecipients(string accountId, string envelopeId, EnvelopeRecipients requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeRecipients>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeRecipients>(response)!;
    }

    public EnvelopeRecipients RecipientsDeleteRecipient(string accountId, string envelopeId, string recipientId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients[recipientId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeRecipients>(response)!;
    }

    public ConsumerDisclosure ConsumerDisclosureGetConsumerDisclosureEn_4df2ebf1(string accountId, string envelopeId, string recipientId, string langCode = "", bool includeLangCode = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients[recipientId].Consumer_disclosure;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeLangCode)
            {
                config.QueryParameters.LangCode = langCode;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ConsumerDisclosure>(response)!;
    }

    public ConsumerDisclosure ConsumerDisclosureGetConsumerDisclosureEn_1d97a7ba(string accountId, string envelopeId, string langCode, string recipientId, string langCode2 = "", bool includeLangCode = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients[recipientId].Consumer_disclosure[langCode];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeLangCode)
            {
                config.QueryParameters.LangCode = langCode2;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ConsumerDisclosure>(response)!;
    }

    public DocumentVisibilityList RecipientsGetRecipientDocumentVisibility(string accountId, string envelopeId, string recipientId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients[recipientId].Document_visibility;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DocumentVisibilityList>(response)!;
    }

    public DocumentVisibilityList RecipientsPutRecipientDocumentVisibility(string accountId, string envelopeId, string recipientId, DocumentVisibilityList requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients[recipientId].Document_visibility;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.DocumentVisibilityList>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DocumentVisibilityList>(response)!;
    }

    public IdEvidenceResourceToken RecipientsPostRecipientProofFileResourceToken(string accountId, string envelopeId, string recipientId, string tokenScopes = "", bool includeTokenScopes = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients[recipientId].Identity_proof_token;
        var response = requestBuilder.PostAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<IdEvidenceResourceToken>(response)!;
    }

    public byte[] RecipientsGetRecipientInitialsImage(string accountId, string envelopeId, string recipientId, string includeChrome = "", bool includeIncludeChrome = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients[recipientId].Initials_image;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeIncludeChrome)
            {
                config.QueryParameters.IncludeChrome = includeChrome;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public byte[] RecipientsPutRecipientInitialsImage(string accountId, string envelopeId, string recipientId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients[recipientId].Initials_image;
        var response = requestBuilder.PutAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public UserSignature RecipientsGetRecipientSignature(string accountId, string envelopeId, string recipientId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients[recipientId].Signature;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserSignature>(response)!;
    }

    public byte[] RecipientsGetRecipientSignatureImage(string accountId, string envelopeId, string recipientId, string includeChrome = "", bool includeIncludeChrome = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients[recipientId].Signature_image;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeIncludeChrome)
            {
                config.QueryParameters.IncludeChrome = includeChrome;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public byte[] RecipientsPutRecipientSignatureImage(string accountId, string envelopeId, string recipientId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients[recipientId].Signature_image;
        var response = requestBuilder.PutAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public EnvelopeRecipientTabs RecipientsGetRecipientTabs(string accountId, string envelopeId, string recipientId, string includeAnchorTabLocations = "", bool includeIncludeAnchorTabLocations = false, string includeMetadata = "", bool includeIncludeMetadata = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients[recipientId].Tabs;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeIncludeAnchorTabLocations)
            {
                config.QueryParameters.IncludeAnchorTabLocations = includeAnchorTabLocations;
            }
            if (includeIncludeMetadata)
            {
                config.QueryParameters.IncludeMetadata = includeMetadata;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeRecipientTabs>(response)!;
    }

    public EnvelopeRecipientTabs RecipientsPutRecipientTabs(string accountId, string envelopeId, string recipientId, EnvelopeRecipientTabs requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients[recipientId].Tabs;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeRecipientTabs>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeRecipientTabs>(response)!;
    }

    public EnvelopeRecipientTabs RecipientsPostRecipientTabs(string accountId, string envelopeId, string recipientId, EnvelopeRecipientTabs requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients[recipientId].Tabs;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeRecipientTabs>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeRecipientTabs>(response)!;
    }

    public EnvelopeRecipientTabs RecipientsDeleteRecipientTabs(string accountId, string envelopeId, string recipientId, EnvelopeRecipientTabs requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients[recipientId].Tabs;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeRecipientTabs>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeRecipientTabs>(response)!;
    }

    public ViewUrl ViewsPostRecipientManualReviewView(string accountId, string envelopeId, string recipientId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients[recipientId].Views.Identity_manual_review;
        var response = requestBuilder.PostAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ViewUrl>(response)!;
    }

    public DocumentVisibilityList RecipientsPutRecipientsDocumentVisibility(string accountId, string envelopeId, DocumentVisibilityList requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Recipients.Document_visibility;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.DocumentVisibilityList>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DocumentVisibilityList>(response)!;
    }

    public DocumentHtmlDefinitions ResponsiveHtmlPostResponsiveHtmlPreview(string accountId, string envelopeId, DocumentHtmlDefinition requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Responsive_html_preview;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.DocumentHtmlDefinition>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DocumentHtmlDefinitions>(response)!;
    }

    public EnvelopesSharesResponse EnvelopesSharesPostEnvelopesShares(string accountId, string envelopeId, EnvelopesSharesRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Shares;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopesSharesRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopesSharesResponse>(response)!;
    }

    public byte[] TabsBlobGetTabsBlob(string accountId, string envelopeId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Tabs_blob;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public byte[] TabsBlobPutTabsBlob(string accountId, string envelopeId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Tabs_blob;
        var response = requestBuilder.PutAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public TemplateInformation TemplatesGetEnvelopeTemplates(string accountId, string envelopeId, string include = "", bool includeInclude = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Templates;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeInclude)
            {
                config.QueryParameters.Include = include;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<TemplateInformation>(response)!;
    }

    public DocumentTemplateList TemplatesPostEnvelopeTemplates(string accountId, string envelopeId, DocumentTemplateList requestBody, string preserveTemplateRecipient = "", bool includePreserveTemplateRecipient = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Templates;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.DocumentTemplateList>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DocumentTemplateList>(response)!;
    }

    public EnvelopeViews ViewsPostEnvelopeCorrectView(string accountId, string envelopeId, EnvelopeViewRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Views.Correct;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeViewRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeViews>(response)!;
    }

    public byte[] ViewsDeleteEnvelopeCorrectView(string accountId, string envelopeId, CorrectViewRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Views.Correct;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.CorrectViewRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public EnvelopeViews ViewsPostEnvelopeEditView(string accountId, string envelopeId, EnvelopeViewRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Views.Edit;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeViewRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeViews>(response)!;
    }

    public EnvelopeViews ViewsPostEnvelopeRecipientView(string accountId, string envelopeId, RecipientViewRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Views.Recipient;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.RecipientViewRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeViews>(response)!;
    }

    public ViewUrl ViewsPostEnvelopeRecipientPreview(string accountId, string envelopeId, RecipientPreviewRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Views.Recipient_preview;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.RecipientPreviewRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ViewUrl>(response)!;
    }

    public EnvelopeViews ViewsPostEnvelopeSenderView(string accountId, string envelopeId, EnvelopeViewRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Views.Sender;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeViewRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeViews>(response)!;
    }

    public ViewUrl ViewsPostEnvelopeRecipientSharedView(string accountId, string envelopeId, RecipientViewRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Views.Shared;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.RecipientViewRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ViewUrl>(response)!;
    }

    public Workflow EnvelopeWorkflowDefinitionV2GetEnvelopeWo_ca403e5e(string accountId, string envelopeId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Workflow;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Workflow>(response)!;
    }

    public Workflow EnvelopeWorkflowDefinitionV2PutEnvelopeWo_91343534(string accountId, string envelopeId, Workflow requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Workflow;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.Workflow>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Workflow>(response)!;
    }

    public byte[] EnvelopeWorkflowDefinitionV2DeleteEnvelop_21f585c7(string accountId, string envelopeId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Workflow;
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public ScheduledSending EnvelopeWorkflowScheduledSendingGetEnvelo_5f6bd7d7(string accountId, string envelopeId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Workflow.ScheduledSending;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ScheduledSending>(response)!;
    }

    public ScheduledSending EnvelopeWorkflowScheduledSendingPutEnvelo_ae0bb984(string accountId, string envelopeId, ScheduledSending requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Workflow.ScheduledSending;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.ScheduledSending>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ScheduledSending>(response)!;
    }

    public byte[] EnvelopeWorkflowScheduledSendingDeleteEnv_a15e6531(string accountId, string envelopeId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Workflow.ScheduledSending;
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public WorkflowStep EnvelopeWorkflowStepPostEnvelopeWorkflowS_4ee02cd5(string accountId, string envelopeId, WorkflowStep requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Workflow.Steps;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.WorkflowStep>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<WorkflowStep>(response)!;
    }

    public WorkflowStep EnvelopeWorkflowStepGetEnvelopeWorkflowSt_26375df7(string accountId, string envelopeId, string workflowStepId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Workflow.Steps[workflowStepId];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<WorkflowStep>(response)!;
    }

    public WorkflowStep EnvelopeWorkflowStepPutEnvelopeWorkflowSt_7129e3b9(string accountId, string envelopeId, string workflowStepId, WorkflowStep requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Workflow.Steps[workflowStepId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.WorkflowStep>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<WorkflowStep>(response)!;
    }

    public byte[] EnvelopeWorkflowStepDeleteEnvelopeWorkflo_c04fee5e(string accountId, string envelopeId, string workflowStepId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Workflow.Steps[workflowStepId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public DelayedRouting EnvelopeWorkflowDelayedRoutingGetEnvelope_4a07658d(string accountId, string envelopeId, string workflowStepId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Workflow.Steps[workflowStepId].DelayedRouting;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DelayedRouting>(response)!;
    }

    public DelayedRouting EnvelopeWorkflowDelayedRoutingPutEnvelope_8192b9fd(string accountId, string envelopeId, string workflowStepId, DelayedRouting requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Workflow.Steps[workflowStepId].DelayedRouting;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.DelayedRouting>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DelayedRouting>(response)!;
    }

    public byte[] EnvelopeWorkflowDelayedRoutingDeleteEnvel_9260c986(string accountId, string envelopeId, string workflowStepId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes[envelopeId].Workflow.Steps[workflowStepId].DelayedRouting;
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public EnvelopesInformation EnvelopesPutStatus(string accountId, EnvelopeIdsRequest requestBody, string acStatus = "", bool includeAcStatus = false, string block = "", bool includeBlock = false, string count = "", bool includeCount = false, string email = "", bool includeEmail = false, string envelopeIds = "", bool includeEnvelopeIds = false, string fromDate = "", bool includeFromDate = false, string fromToStatus = "", bool includeFromToStatus = false, string startPosition = "", bool includeStartPosition = false, string status = "", bool includeStatus = false, string toDate = "", bool includeToDate = false, string transactionIds = "", bool includeTransactionIds = false, string userName = "", bool includeUserName = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes.Status;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeIdsRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopesInformation>(response)!;
    }

    public EnvelopeTransferRuleInformation EnvelopeTransferRulesGetEnvelopeTransferRules(string accountId, string count = "", bool includeCount = false, string startPosition = "", bool includeStartPosition = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes.Transfer_rules;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeTransferRuleInformation>(response)!;
    }

    public EnvelopeTransferRuleInformation EnvelopeTransferRulesPutEnvelopeTransferRules(string accountId, EnvelopeTransferRuleInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes.Transfer_rules;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeTransferRuleInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeTransferRuleInformation>(response)!;
    }

    public EnvelopeTransferRuleInformation EnvelopeTransferRulesPostEnvelopeTransferRules(string accountId, EnvelopeTransferRuleRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes.Transfer_rules;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeTransferRuleRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeTransferRuleInformation>(response)!;
    }

    public EnvelopeTransferRule EnvelopeTransferRulesPutEnvelopeTransferRule(string accountId, string envelopeTransferRuleId, EnvelopeTransferRule requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes.Transfer_rules[envelopeTransferRuleId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeTransferRule>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeTransferRule>(response)!;
    }

    public byte[] EnvelopeTransferRulesDeleteEnvelopeTransferRules(string accountId, string envelopeTransferRuleId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Envelopes.Transfer_rules[envelopeTransferRuleId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public FavoriteTemplatesInfo FavoriteTemplatesGetFavoriteTemplates(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Favorite_templates;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<FavoriteTemplatesInfo>(response)!;
    }

    public FavoriteTemplatesInfo FavoriteTemplatesPutFavoriteTemplate(string accountId, FavoriteTemplatesInfo requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Favorite_templates;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.FavoriteTemplatesInfo>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<FavoriteTemplatesInfo>(response)!;
    }

    public FavoriteTemplatesInfo FavoriteTemplatesUnFavoriteTemplate(string accountId, FavoriteTemplatesInfo requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Favorite_templates;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.FavoriteTemplatesInfo>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<FavoriteTemplatesInfo>(response)!;
    }

    public FoldersResponse FoldersGetFolders(string accountId, string count = "", bool includeCount = false, string include = "", bool includeInclude = false, string includeItems = "", bool includeIncludeItems = false, string startPosition = "", bool includeStartPosition = false, string subFolderDepth = "", bool includeSubFolderDepth = false, string template = "", bool includeTemplate = false, string userFilter = "", bool includeUserFilter = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Folders;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeInclude)
            {
                config.QueryParameters.Include = include;
            }
            if (includeIncludeItems)
            {
                config.QueryParameters.IncludeItems = includeItems;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
            if (includeSubFolderDepth)
            {
                config.QueryParameters.SubFolderDepth = subFolderDepth;
            }
            if (includeTemplate)
            {
                config.QueryParameters.Template = template;
            }
            if (includeUserFilter)
            {
                config.QueryParameters.UserFilter = userFilter;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<FoldersResponse>(response)!;
    }

    public FolderItemsResponse FoldersGetFolderItems(string accountId, string folderId, string fromDate = "", bool includeFromDate = false, string includeItems = "", bool includeIncludeItems = false, string ownerEmail = "", bool includeOwnerEmail = false, string ownerName = "", bool includeOwnerName = false, string searchText = "", bool includeSearchText = false, string startPosition = "", bool includeStartPosition = false, string status = "", bool includeStatus = false, string toDate = "", bool includeToDate = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Folders[folderId];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeFromDate)
            {
                config.QueryParameters.FromDate = fromDate;
            }
            if (includeIncludeItems)
            {
                config.QueryParameters.IncludeItems = includeItems;
            }
            if (includeOwnerEmail)
            {
                config.QueryParameters.OwnerEmail = ownerEmail;
            }
            if (includeOwnerName)
            {
                config.QueryParameters.OwnerName = ownerName;
            }
            if (includeSearchText)
            {
                config.QueryParameters.SearchText = searchText;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
            if (includeStatus)
            {
                config.QueryParameters.Status = status;
            }
            if (includeToDate)
            {
                config.QueryParameters.ToDate = toDate;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<FolderItemsResponse>(response)!;
    }

    public FoldersResponse FoldersPutFolderById(string accountId, string folderId, FoldersRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Folders[folderId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.FoldersRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<FoldersResponse>(response)!;
    }

    public GroupInformation GroupsGetGroups(string accountId, string count = "", bool includeCount = false, string groupType = "", bool includeGroupType = false, string includeUsercount = "", bool includeIncludeUsercount = false, string searchText = "", bool includeSearchText = false, string startPosition = "", bool includeStartPosition = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Groups;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeGroupType)
            {
                config.QueryParameters.GroupType = groupType;
            }
            if (includeIncludeUsercount)
            {
                config.QueryParameters.IncludeUsercount = includeUsercount;
            }
            if (includeSearchText)
            {
                config.QueryParameters.SearchText = searchText;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<GroupInformation>(response)!;
    }

    public GroupInformation GroupsPutGroups(string accountId, GroupInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Groups;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.GroupInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<GroupInformation>(response)!;
    }

    public GroupInformation GroupsPostGroups(string accountId, GroupInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Groups;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.GroupInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<GroupInformation>(response)!;
    }

    public GroupInformation GroupsDeleteGroups(string accountId, GroupInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Groups;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.GroupInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<GroupInformation>(response)!;
    }

    public GroupBrands BrandsGetGroupBrands(string accountId, string groupId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Groups[groupId].Brands;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<GroupBrands>(response)!;
    }

    public GroupBrands BrandsPutGroupBrands(string accountId, string groupId, BrandsRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Groups[groupId].Brands;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.BrandsRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<GroupBrands>(response)!;
    }

    public GroupBrands BrandsDeleteGroupBrands(string accountId, string groupId, BrandsRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Groups[groupId].Brands;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.BrandsRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<GroupBrands>(response)!;
    }

    public UsersResponse GroupsGetGroupUsers(string accountId, string groupId, string count = "", bool includeCount = false, string startPosition = "", bool includeStartPosition = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Groups[groupId].Users;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UsersResponse>(response)!;
    }

    public UsersResponse GroupsPutGroupUsers(string accountId, string groupId, UserInfoList requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Groups[groupId].Users;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.UserInfoList>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UsersResponse>(response)!;
    }

    public UsersResponse GroupsDeleteGroupUsers(string accountId, string groupId, UserInfoList requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Groups[groupId].Users;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.UserInfoList>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UsersResponse>(response)!;
    }

    public AccountIdentityVerificationResponse AccountIdentityVerificationGetAccountIden_717a34ef(string accountId, string identityVerificationWorkflowStatus = "", bool includeIdentityVerificationWorkflowStatus = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Identity_verification;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeIdentityVerificationWorkflowStatus)
            {
                config.QueryParameters.IdentityVerificationWorkflowStatus = identityVerificationWorkflowStatus;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountIdentityVerificationResponse>(response)!;
    }

    public PaymentGatewayAccountsInfo PaymentGatewayAccountsGetAllPaymentGatewayAccounts(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Payment_gateway_accounts;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<PaymentGatewayAccountsInfo>(response)!;
    }

    public PermissionProfileInformation PermissionProfilesGetPermissionProfiles(string accountId, string include = "", bool includeInclude = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Permission_profiles;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeInclude)
            {
                config.QueryParameters.Include = include;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<PermissionProfileInformation>(response)!;
    }

    public PermissionProfile PermissionProfilesPostPermissionProfiles(string accountId, PermissionProfile requestBody, string include = "", bool includeInclude = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Permission_profiles;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.PermissionProfile>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<PermissionProfile>(response)!;
    }

    public PermissionProfile PermissionProfilesGetPermissionProfile(string accountId, string permissionProfileId, string include = "", bool includeInclude = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Permission_profiles[permissionProfileId];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeInclude)
            {
                config.QueryParameters.Include = include;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<PermissionProfile>(response)!;
    }

    public PermissionProfile PermissionProfilesPutPermissionProfiles(string accountId, string permissionProfileId, PermissionProfile requestBody, string include = "", bool includeInclude = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Permission_profiles[permissionProfileId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.PermissionProfile>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<PermissionProfile>(response)!;
    }

    public byte[] PermissionProfilesDeletePermissionProfiles(string accountId, string permissionProfileId, string moveUsersTo = "", bool includeMoveUsersTo = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Permission_profiles[permissionProfileId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public PowerFormsResponse PowerFormsGetPowerFormsList(string accountId, string count = "", bool includeCount = false, string fromDate = "", bool includeFromDate = false, string order = "", bool includeOrder = false, string orderBy = "", bool includeOrderBy = false, string searchFields = "", bool includeSearchFields = false, string searchText = "", bool includeSearchText = false, string startPosition = "", bool includeStartPosition = false, string toDate = "", bool includeToDate = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Powerforms;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeFromDate)
            {
                config.QueryParameters.FromDate = fromDate;
            }
            if (includeOrder)
            {
                config.QueryParameters.Order = order;
            }
            if (includeOrderBy)
            {
                config.QueryParameters.OrderBy = orderBy;
            }
            if (includeSearchFields)
            {
                config.QueryParameters.SearchFields = searchFields;
            }
            if (includeSearchText)
            {
                config.QueryParameters.SearchText = searchText;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
            if (includeToDate)
            {
                config.QueryParameters.ToDate = toDate;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<PowerFormsResponse>(response)!;
    }

    public PowerForm PowerFormsPostPowerForm(string accountId, PowerForm requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Powerforms;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.PowerForm>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<PowerForm>(response)!;
    }

    public PowerFormsResponse PowerFormsDeletePowerFormsList(string accountId, PowerFormsRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Powerforms;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.PowerFormsRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<PowerFormsResponse>(response)!;
    }

    public PowerForm PowerFormsGetPowerForm(string accountId, string powerFormId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Powerforms[powerFormId];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<PowerForm>(response)!;
    }

    public PowerForm PowerFormsPutPowerForm(string accountId, string powerFormId, PowerForm requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Powerforms[powerFormId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.PowerForm>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<PowerForm>(response)!;
    }

    public byte[] PowerFormsDeletePowerForm(string accountId, string powerFormId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Powerforms[powerFormId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public PowerFormsFormDataResponse PowerFormsGetPowerFormFormData(string accountId, string powerFormId, string dataLayout = "", bool includeDataLayout = false, string fromDate = "", bool includeFromDate = false, string toDate = "", bool includeToDate = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Powerforms[powerFormId].Form_data;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeDataLayout)
            {
                config.QueryParameters.DataLayout = dataLayout;
            }
            if (includeFromDate)
            {
                config.QueryParameters.FromDate = fromDate;
            }
            if (includeToDate)
            {
                config.QueryParameters.ToDate = toDate;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<PowerFormsFormDataResponse>(response)!;
    }

    public PowerFormSendersResponse PowerFormsGetPowerFormsSenders(string accountId, string startPosition = "", bool includeStartPosition = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Powerforms.Senders;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<PowerFormSendersResponse>(response)!;
    }

    public RecipientNamesResponse RecipientNamesGetRecipientNames(string accountId, string email = "", bool includeEmail = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Recipient_names;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeEmail)
            {
                config.QueryParameters.Email = email;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<RecipientNamesResponse>(response)!;
    }

    public AccountSealProviders AccountSignatureProvidersGetSealProviders(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Seals;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountSealProviders>(response)!;
    }

    public FolderItemResponse SearchFoldersGetSearchFolderContents(string accountId, string searchFolderId, string all = "", bool includeAll = false, string count = "", bool includeCount = false, string fromDate = "", bool includeFromDate = false, string includeRecipients = "", bool includeIncludeRecipients = false, string order = "", bool includeOrder = false, string orderBy = "", bool includeOrderBy = false, string startPosition = "", bool includeStartPosition = false, string toDate = "", bool includeToDate = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Search_folders[searchFolderId];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeAll)
            {
                config.QueryParameters.All = all;
            }
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeFromDate)
            {
                config.QueryParameters.FromDate = fromDate;
            }
            if (includeIncludeRecipients)
            {
                config.QueryParameters.IncludeRecipients = includeRecipients;
            }
            if (includeOrder)
            {
                config.QueryParameters.Order = order;
            }
            if (includeOrderBy)
            {
                config.QueryParameters.OrderBy = orderBy;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
            if (includeToDate)
            {
                config.QueryParameters.ToDate = toDate;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<FolderItemResponse>(response)!;
    }

    public AccountSettingsInformation SettingsGetSettings(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Settings;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountSettingsInformation>(response)!;
    }

    public byte[] SettingsPutSettings(string accountId, AccountSettingsInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Settings;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.AccountSettingsInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public BccEmailArchiveList BCCEmailArchiveGetBCCEmailArchiveList(string accountId, string count = "", bool includeCount = false, string startPosition = "", bool includeStartPosition = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Settings.Bcc_email_archives;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BccEmailArchiveList>(response)!;
    }

    public BccEmailArchive BCCEmailArchivePostBCCEmailArchive(string accountId, BccEmailArchive requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Settings.Bcc_email_archives;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.BccEmailArchive>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BccEmailArchive>(response)!;
    }

    public BccEmailArchiveHistoryList BCCEmailArchiveGetBCCEmailArchiveHistoryList(string accountId, string bccEmailArchiveId, string count = "", bool includeCount = false, string startPosition = "", bool includeStartPosition = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Settings.Bcc_email_archives[bccEmailArchiveId];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BccEmailArchiveHistoryList>(response)!;
    }

    public byte[] BCCEmailArchiveDeleteBCCEmailArchive(string accountId, string bccEmailArchiveId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Settings.Bcc_email_archives[bccEmailArchiveId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public ENoteConfiguration ENoteConfigurationGetENoteConfiguration(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Settings.Enote_configuration;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ENoteConfiguration>(response)!;
    }

    public ENoteConfiguration ENoteConfigurationPutENoteConfiguration(string accountId, ENoteConfiguration requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Settings.Enote_configuration;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.ENoteConfiguration>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ENoteConfiguration>(response)!;
    }

    public byte[] ENoteConfigurationDeleteENoteConfiguration(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Settings.Enote_configuration;
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public EnvelopePurgeConfiguration EnvelopePurgeConfigurationGetEnvelopePurg_9c571224(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Settings.Envelope_purge_configuration;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopePurgeConfiguration>(response)!;
    }

    public EnvelopePurgeConfiguration EnvelopePurgeConfigurationPutEnvelopePurg_80450268(string accountId, EnvelopePurgeConfiguration requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Settings.Envelope_purge_configuration;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopePurgeConfiguration>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopePurgeConfiguration>(response)!;
    }

    public NotificationDefaults NotificationDefaultsGetNotificationDefaults(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Settings.Notification_defaults;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<NotificationDefaults>(response)!;
    }

    public NotificationDefaults NotificationDefaultsPutNotificationDefaults(string accountId, NotificationDefaults requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Settings.Notification_defaults;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.NotificationDefaults>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<NotificationDefaults>(response)!;
    }

    public AccountPasswordRules AccountPasswordRulesGetAccountPasswordRules(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Settings.Password_rules;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountPasswordRules>(response)!;
    }

    public AccountPasswordRules AccountPasswordRulesPutAccountPasswordRules(string accountId, AccountPasswordRules requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Settings.Password_rules;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.AccountPasswordRules>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountPasswordRules>(response)!;
    }

    public TabAccountSettings TabSettingsGetTabSettings(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Settings.Tabs;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<TabAccountSettings>(response)!;
    }

    public TabAccountSettings TabSettingsPutSettings(string accountId, TabAccountSettings requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Settings.Tabs;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TabAccountSettings>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<TabAccountSettings>(response)!;
    }

    public AccountSharedAccess SharedAccessGetSharedAccess(string accountId, string count = "", bool includeCount = false, string envelopesNotSharedUserStatus = "", bool includeEnvelopesNotSharedUserStatus = false, string folderIds = "", bool includeFolderIds = false, string itemType = "", bool includeItemType = false, string searchText = "", bool includeSearchText = false, string shared = "", bool includeShared = false, string startPosition = "", bool includeStartPosition = false, string userIds = "", bool includeUserIds = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Shared_access;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeEnvelopesNotSharedUserStatus)
            {
                config.QueryParameters.EnvelopesNotSharedUserStatus = envelopesNotSharedUserStatus;
            }
            if (includeFolderIds)
            {
                config.QueryParameters.FolderIds = folderIds;
            }
            if (includeItemType)
            {
                config.QueryParameters.ItemType = itemType;
            }
            if (includeSearchText)
            {
                config.QueryParameters.SearchText = searchText;
            }
            if (includeShared)
            {
                config.QueryParameters.Shared = shared;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
            if (includeUserIds)
            {
                config.QueryParameters.UserIds = userIds;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountSharedAccess>(response)!;
    }

    public AccountSharedAccess SharedAccessPutSharedAccess(string accountId, AccountSharedAccess requestBody, string itemType = "", bool includeItemType = false, string preserveExistingSharedAccess = "", bool includePreserveExistingSharedAccess = false, string userIds = "", bool includeUserIds = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Shared_access;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.AccountSharedAccess>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountSharedAccess>(response)!;
    }

    public AccountSignatureProviders AccountSignatureProvidersGetSignatureProviders(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].SignatureProviders;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountSignatureProviders>(response)!;
    }

    public AccountSignaturesInformation AccountSignaturesGetAccountSignatures(string accountId, string stampFormat = "", bool includeStampFormat = false, string stampName = "", bool includeStampName = false, string stampType = "", bool includeStampType = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Signatures;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeStampFormat)
            {
                config.QueryParameters.StampFormat = stampFormat;
            }
            if (includeStampName)
            {
                config.QueryParameters.StampName = stampName;
            }
            if (includeStampType)
            {
                config.QueryParameters.StampType = stampType;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountSignaturesInformation>(response)!;
    }

    public AccountSignaturesInformation AccountSignaturesPutAccountSignature(string accountId, AccountSignaturesInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Signatures;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.AccountSignaturesInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountSignaturesInformation>(response)!;
    }

    public AccountSignaturesInformation AccountSignaturesPostAccountSignatures(string accountId, AccountSignaturesInformation requestBody, string decodeOnly = "", bool includeDecodeOnly = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Signatures;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.AccountSignaturesInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountSignaturesInformation>(response)!;
    }

    public AccountSignature AccountSignaturesGetAccountSignature(string accountId, string signatureId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Signatures[signatureId];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountSignature>(response)!;
    }

    public AccountSignature AccountSignaturesPutAccountSignatureById(string accountId, string signatureId, AccountSignatureDefinition requestBody, string closeExistingSignature = "", bool includeCloseExistingSignature = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Signatures[signatureId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.AccountSignatureDefinition>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountSignature>(response)!;
    }

    public byte[] AccountSignaturesDeleteAccountSignature(string accountId, string signatureId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Signatures[signatureId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public byte[] AccountSignaturesGetAccountSignatureImage(string accountId, string imageType, string signatureId, string includeChrome = "", bool includeIncludeChrome = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Signatures[signatureId][imageType];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeIncludeChrome)
            {
                config.QueryParameters.IncludeChrome = includeChrome;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public AccountSignature AccountSignaturesPutAccountSignatureImage(string accountId, string imageType, string signatureId, string transparentPng = "", bool includeTransparentPng = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Signatures[signatureId][imageType];
        var response = requestBuilder.PutAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountSignature>(response)!;
    }

    public AccountSignature AccountSignaturesDeleteAccountSignatureImage(string accountId, string imageType, string signatureId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Signatures[signatureId][imageType];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<AccountSignature>(response)!;
    }

    public SigningGroupInformation SigningGroupsGetSigningGroups(string accountId, string groupType = "", bool includeGroupType = false, string includeUsers = "", bool includeIncludeUsers = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Signing_groups;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeGroupType)
            {
                config.QueryParameters.GroupType = groupType;
            }
            if (includeIncludeUsers)
            {
                config.QueryParameters.IncludeUsers = includeUsers;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<SigningGroupInformation>(response)!;
    }

    public SigningGroupInformation SigningGroupsPutSigningGroups(string accountId, SigningGroupInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Signing_groups;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.SigningGroupInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<SigningGroupInformation>(response)!;
    }

    public SigningGroupInformation SigningGroupsPostSigningGroups(string accountId, SigningGroupInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Signing_groups;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.SigningGroupInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<SigningGroupInformation>(response)!;
    }

    public SigningGroupInformation SigningGroupsDeleteSigningGroups(string accountId, SigningGroupInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Signing_groups;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.SigningGroupInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<SigningGroupInformation>(response)!;
    }

    public SigningGroup SigningGroupsGetSigningGroup(string accountId, string signingGroupId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Signing_groups[signingGroupId];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<SigningGroup>(response)!;
    }

    public SigningGroup SigningGroupsPutSigningGroup(string accountId, string signingGroupId, SigningGroup requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Signing_groups[signingGroupId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.SigningGroup>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<SigningGroup>(response)!;
    }

    public SigningGroupUsers SigningGroupsGetSigningGroupUsers(string accountId, string signingGroupId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Signing_groups[signingGroupId].Users;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<SigningGroupUsers>(response)!;
    }

    public SigningGroupUsers SigningGroupsPutSigningGroupUsers(string accountId, string signingGroupId, SigningGroupUsers requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Signing_groups[signingGroupId].Users;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.SigningGroupUsers>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<SigningGroupUsers>(response)!;
    }

    public SigningGroupUsers SigningGroupsDeleteSigningGroupUsers(string accountId, string signingGroupId, SigningGroupUsers requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Signing_groups[signingGroupId].Users;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.SigningGroupUsers>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<SigningGroupUsers>(response)!;
    }

    public SupportedLanguages SupportedLanguagesGetSupportedLanguages(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Supported_languages;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<SupportedLanguages>(response)!;
    }

    public TabMetadataList TabsGetTabDefinitions(string accountId, string customTabOnly = "", bool includeCustomTabOnly = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Tab_definitions;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCustomTabOnly)
            {
                config.QueryParameters.CustomTabOnly = customTabOnly;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<TabMetadataList>(response)!;
    }

    public TabMetadata TabsPostTabDefinitions(string accountId, TabMetadata requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Tab_definitions;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TabMetadata>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<TabMetadata>(response)!;
    }

    public TabMetadata TabGetCustomTab(string accountId, string customTabId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Tab_definitions[customTabId];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<TabMetadata>(response)!;
    }

    public TabMetadata TabPutCustomTab(string accountId, string customTabId, TabMetadata requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Tab_definitions[customTabId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TabMetadata>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<TabMetadata>(response)!;
    }

    public byte[] TabDeleteCustomTab(string accountId, string customTabId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Tab_definitions[customTabId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public EnvelopeTemplateResults TemplatesGetTemplates(string accountId, string count = "", bool includeCount = false, string createdFromDate = "", bool includeCreatedFromDate = false, string createdToDate = "", bool includeCreatedToDate = false, string folderIds = "", bool includeFolderIds = false, string folderTypes = "", bool includeFolderTypes = false, string fromDate = "", bool includeFromDate = false, string include = "", bool includeInclude = false, string isDeletedTemplateOnly = "", bool includeIsDeletedTemplateOnly = false, string isDownload = "", bool includeIsDownload = false, string modifiedFromDate = "", bool includeModifiedFromDate = false, string modifiedToDate = "", bool includeModifiedToDate = false, string order = "", bool includeOrder = false, string orderBy = "", bool includeOrderBy = false, string searchFields = "", bool includeSearchFields = false, string searchText = "", bool includeSearchText = false, string sharedByMe = "", bool includeSharedByMe = false, string startPosition = "", bool includeStartPosition = false, string templateIds = "", bool includeTemplateIds = false, string toDate = "", bool includeToDate = false, string usedFromDate = "", bool includeUsedFromDate = false, string usedToDate = "", bool includeUsedToDate = false, string userFilter = "", bool includeUserFilter = false, string userId = "", bool includeUserId = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeCreatedFromDate)
            {
                config.QueryParameters.CreatedFromDate = createdFromDate;
            }
            if (includeCreatedToDate)
            {
                config.QueryParameters.CreatedToDate = createdToDate;
            }
            if (includeFolderIds)
            {
                config.QueryParameters.FolderIds = folderIds;
            }
            if (includeFolderTypes)
            {
                config.QueryParameters.FolderTypes = folderTypes;
            }
            if (includeFromDate)
            {
                config.QueryParameters.FromDate = fromDate;
            }
            if (includeInclude)
            {
                config.QueryParameters.Include = include;
            }
            if (includeIsDeletedTemplateOnly)
            {
                config.QueryParameters.IsDeletedTemplateOnly = isDeletedTemplateOnly;
            }
            if (includeIsDownload)
            {
                config.QueryParameters.IsDownload = isDownload;
            }
            if (includeModifiedFromDate)
            {
                config.QueryParameters.ModifiedFromDate = modifiedFromDate;
            }
            if (includeModifiedToDate)
            {
                config.QueryParameters.ModifiedToDate = modifiedToDate;
            }
            if (includeOrder)
            {
                config.QueryParameters.Order = order;
            }
            if (includeOrderBy)
            {
                config.QueryParameters.OrderBy = orderBy;
            }
            if (includeSearchFields)
            {
                config.QueryParameters.SearchFields = searchFields;
            }
            if (includeSearchText)
            {
                config.QueryParameters.SearchText = searchText;
            }
            if (includeSharedByMe)
            {
                config.QueryParameters.SharedByMe = sharedByMe;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
            if (includeTemplateIds)
            {
                config.QueryParameters.TemplateIds = templateIds;
            }
            if (includeToDate)
            {
                config.QueryParameters.ToDate = toDate;
            }
            if (includeUsedFromDate)
            {
                config.QueryParameters.UsedFromDate = usedFromDate;
            }
            if (includeUsedToDate)
            {
                config.QueryParameters.UsedToDate = usedToDate;
            }
            if (includeUserFilter)
            {
                config.QueryParameters.UserFilter = userFilter;
            }
            if (includeUserId)
            {
                config.QueryParameters.UserId = userId;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeTemplateResults>(response)!;
    }

    public TemplateAutoMatchList TemplatesPutTemplates(string accountId, TemplateAutoMatchList requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TemplateAutoMatchList>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<TemplateAutoMatchList>(response)!;
    }

    public TemplateSummary TemplatesPostTemplates(string accountId, EnvelopeTemplate requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeTemplate>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<TemplateSummary>(response)!;
    }

    public EnvelopeTemplate TemplatesGetTemplate(string accountId, string templateId, string include = "", bool includeInclude = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeInclude)
            {
                config.QueryParameters.Include = include;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeTemplate>(response)!;
    }

    public TemplateUpdateSummary TemplatesPutTemplate(string accountId, string templateId, EnvelopeTemplate requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeTemplate>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<TemplateUpdateSummary>(response)!;
    }

    public GroupInformation TemplatesPutTemplatePart(string accountId, string templateId, string templatePart, GroupInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId][templatePart];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.GroupInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<GroupInformation>(response)!;
    }

    public GroupInformation TemplatesDeleteTemplatePart(string accountId, string templateId, string templatePart, GroupInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId][templatePart];
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.GroupInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<GroupInformation>(response)!;
    }

    public CustomFields CustomFieldsGetTemplateCustomFields(string accountId, string templateId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Custom_fields;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<CustomFields>(response)!;
    }

    public CustomFields CustomFieldsPutTemplateCustomFields(string accountId, string templateId, TemplateCustomFields requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Custom_fields;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TemplateCustomFields>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<CustomFields>(response)!;
    }

    public CustomFields CustomFieldsPostTemplateCustomFields(string accountId, string templateId, TemplateCustomFields requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Custom_fields;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TemplateCustomFields>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<CustomFields>(response)!;
    }

    public CustomFields CustomFieldsDeleteTemplateCustomFields(string accountId, string templateId, TemplateCustomFields requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Custom_fields;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TemplateCustomFields>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<CustomFields>(response)!;
    }

    public TemplateDocumentsResult DocumentsGetTemplateDocuments(string accountId, string templateId, string includeAgreementType = "", bool includeIncludeAgreementType = false, string includeTabs = "", bool includeIncludeTabs = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Documents;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeIncludeAgreementType)
            {
                config.QueryParameters.IncludeAgreementType = includeAgreementType;
            }
            if (includeIncludeTabs)
            {
                config.QueryParameters.IncludeTabs = includeTabs;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<TemplateDocumentsResult>(response)!;
    }

    public TemplateDocumentsResult DocumentsPutTemplateDocuments(string accountId, string templateId, EnvelopeDefinition requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Documents;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeDefinition>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<TemplateDocumentsResult>(response)!;
    }

    public TemplateDocumentsResult DocumentsDeleteTemplateDocuments(string accountId, string templateId, EnvelopeDefinition requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Documents;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeDefinition>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<TemplateDocumentsResult>(response)!;
    }

    public byte[] DocumentsGetTemplateDocument(string accountId, string documentId, string templateId, string encrypt = "", bool includeEncrypt = false, string fileType = "", bool includeFileType = false, string showChanges = "", bool includeShowChanges = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Documents[documentId];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeEncrypt)
            {
                config.QueryParameters.Encrypt = encrypt;
            }
            if (includeFileType)
            {
                config.QueryParameters.FileType = fileType;
            }
            if (includeShowChanges)
            {
                config.QueryParameters.ShowChanges = showChanges;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public EnvelopeDocument DocumentsPutTemplateDocument(string accountId, string documentId, string templateId, EnvelopeDefinition requestBody, string isEnvelopeDefinition = "", bool includeIsEnvelopeDefinition = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Documents[documentId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.EnvelopeDefinition>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeDocument>(response)!;
    }

    public DocumentFieldsInformation DocumentFieldsGetTemplateDocumentFields(string accountId, string documentId, string templateId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Documents[documentId].Fields;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DocumentFieldsInformation>(response)!;
    }

    public DocumentFieldsInformation DocumentFieldsPutTemplateDocumentFields(string accountId, string documentId, string templateId, DocumentFieldsInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Documents[documentId].Fields;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.DocumentFieldsInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DocumentFieldsInformation>(response)!;
    }

    public DocumentFieldsInformation DocumentFieldsPostTemplateDocumentFields(string accountId, string documentId, string templateId, DocumentFieldsInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Documents[documentId].Fields;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.DocumentFieldsInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DocumentFieldsInformation>(response)!;
    }

    public DocumentFieldsInformation DocumentFieldsDeleteTemplateDocumentFields(string accountId, string documentId, string templateId, DocumentFieldsInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Documents[documentId].Fields;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.DocumentFieldsInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DocumentFieldsInformation>(response)!;
    }

    public DocumentHtmlDefinitionOriginals ResponsiveHtmlGetTemplateDocumentHtmlDefinitions(string accountId, string documentId, string templateId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Documents[documentId].Html_definitions;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DocumentHtmlDefinitionOriginals>(response)!;
    }

    public PageImages PagesGetTemplatePageImages(string accountId, string documentId, string templateId, string count = "", bool includeCount = false, string dpi = "", bool includeDpi = false, string maxHeight = "", bool includeMaxHeight = false, string maxWidth = "", bool includeMaxWidth = false, string nocache = "", bool includeNocache = false, string showChanges = "", bool includeShowChanges = false, string startPosition = "", bool includeStartPosition = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Documents[documentId].Pages;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeDpi)
            {
                config.QueryParameters.Dpi = dpi;
            }
            if (includeMaxHeight)
            {
                config.QueryParameters.MaxHeight = maxHeight;
            }
            if (includeMaxWidth)
            {
                config.QueryParameters.MaxWidth = maxWidth;
            }
            if (includeNocache)
            {
                config.QueryParameters.Nocache = nocache;
            }
            if (includeShowChanges)
            {
                config.QueryParameters.ShowChanges = showChanges;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<PageImages>(response)!;
    }

    public byte[] PagesDeleteTemplatePage(string accountId, string documentId, string pageNumber, string templateId, PageRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Documents[documentId].Pages[pageNumber];
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.PageRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public byte[] PagesGetTemplatePageImage(string accountId, string documentId, string pageNumber, string templateId, string dpi = "", bool includeDpi = false, string maxHeight = "", bool includeMaxHeight = false, string maxWidth = "", bool includeMaxWidth = false, string showChanges = "", bool includeShowChanges = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Documents[documentId].Pages[pageNumber].Page_image;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeDpi)
            {
                config.QueryParameters.Dpi = dpi;
            }
            if (includeMaxHeight)
            {
                config.QueryParameters.MaxHeight = maxHeight;
            }
            if (includeMaxWidth)
            {
                config.QueryParameters.MaxWidth = maxWidth;
            }
            if (includeShowChanges)
            {
                config.QueryParameters.ShowChanges = showChanges;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public byte[] PagesPutTemplatePageImage(string accountId, string documentId, string pageNumber, string templateId, PageRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Documents[documentId].Pages[pageNumber].Page_image;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.PageRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public TemplateDocumentTabs TabsGetTemplatePageTabs(string accountId, string documentId, string pageNumber, string templateId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Documents[documentId].Pages[pageNumber].Tabs;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<TemplateDocumentTabs>(response)!;
    }

    public DocumentHtmlDefinitions ResponsiveHtmlPostTemplateDocumentRespons_7d05547f(string accountId, string documentId, string templateId, DocumentHtmlDefinition requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Documents[documentId].Responsive_html_preview;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.DocumentHtmlDefinition>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DocumentHtmlDefinitions>(response)!;
    }

    public TemplateDocumentTabs TabsGetTemplateDocumentTabs(string accountId, string documentId, string templateId, string pageNumbers = "", bool includePageNumbers = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Documents[documentId].Tabs;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includePageNumbers)
            {
                config.QueryParameters.PageNumbers = pageNumbers;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<TemplateDocumentTabs>(response)!;
    }

    public Tabs TabsPutTemplateDocumentTabs(string accountId, string documentId, string templateId, TemplateTabs requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Documents[documentId].Tabs;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TemplateTabs>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Tabs>(response)!;
    }

    public Tabs TabsPostTemplateDocumentTabs(string accountId, string documentId, string templateId, TemplateTabs requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Documents[documentId].Tabs;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TemplateTabs>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Tabs>(response)!;
    }

    public Tabs TabsDeleteTemplateDocumentTabs(string accountId, string documentId, string templateId, TemplateTabs requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Documents[documentId].Tabs;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TemplateTabs>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Tabs>(response)!;
    }

    public DocumentHtmlDefinitionOriginals ResponsiveHtmlGetTemplateHtmlDefinitions(string accountId, string templateId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Html_definitions;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DocumentHtmlDefinitionOriginals>(response)!;
    }

    public LockInformation LockGetTemplateLock(string accountId, string templateId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Lock;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<LockInformation>(response)!;
    }

    public LockInformation LockPutTemplateLock(string accountId, string templateId, LockRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Lock;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.LockRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<LockInformation>(response)!;
    }

    public LockInformation LockPostTemplateLock(string accountId, string templateId, LockRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Lock;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.LockRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<LockInformation>(response)!;
    }

    public LockInformation LockDeleteTemplateLock(string accountId, string templateId, LockRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Lock;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.LockRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<LockInformation>(response)!;
    }

    public Notification NotificationGetTemplatesTemplateIdNotification(string accountId, string templateId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Notification;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Notification>(response)!;
    }

    public Notification NotificationPutTemplatesTemplateIdNotification(string accountId, string templateId, TemplateNotificationRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Notification;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TemplateNotificationRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Notification>(response)!;
    }

    public Recipients RecipientsGetTemplateRecipients(string accountId, string templateId, string includeAnchorTabLocations = "", bool includeIncludeAnchorTabLocations = false, string includeExtended = "", bool includeIncludeExtended = false, string includeTabs = "", bool includeIncludeTabs = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Recipients;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeIncludeAnchorTabLocations)
            {
                config.QueryParameters.IncludeAnchorTabLocations = includeAnchorTabLocations;
            }
            if (includeIncludeExtended)
            {
                config.QueryParameters.IncludeExtended = includeExtended;
            }
            if (includeIncludeTabs)
            {
                config.QueryParameters.IncludeTabs = includeTabs;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Recipients>(response)!;
    }

    public RecipientsUpdateSummary RecipientsPutTemplateRecipients(string accountId, string templateId, TemplateRecipients requestBody, string resendEnvelope = "", bool includeResendEnvelope = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Recipients;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TemplateRecipients>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<RecipientsUpdateSummary>(response)!;
    }

    public Recipients RecipientsPostTemplateRecipients(string accountId, string templateId, TemplateRecipients requestBody, string resendEnvelope = "", bool includeResendEnvelope = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Recipients;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TemplateRecipients>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Recipients>(response)!;
    }

    public Recipients RecipientsDeleteTemplateRecipients(string accountId, string templateId, TemplateRecipients requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Recipients;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TemplateRecipients>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Recipients>(response)!;
    }

    public Recipients RecipientsDeleteTemplateRecipient(string accountId, string recipientId, string templateId, TemplateRecipients requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Recipients[recipientId];
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TemplateRecipients>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Recipients>(response)!;
    }

    public DocumentVisibilityList RecipientsGetTemplateRecipientDocumentVisibility(string accountId, string recipientId, string templateId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Recipients[recipientId].Document_visibility;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DocumentVisibilityList>(response)!;
    }

    public TemplateDocumentVisibilityList RecipientsPutTemplateRecipientDocumentVisibility(string accountId, string recipientId, string templateId, TemplateDocumentVisibilityList requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Recipients[recipientId].Document_visibility;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TemplateDocumentVisibilityList>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<TemplateDocumentVisibilityList>(response)!;
    }

    public Tabs RecipientsGetTemplateRecipientTabs(string accountId, string recipientId, string templateId, string includeAnchorTabLocations = "", bool includeIncludeAnchorTabLocations = false, string includeMetadata = "", bool includeIncludeMetadata = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Recipients[recipientId].Tabs;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeIncludeAnchorTabLocations)
            {
                config.QueryParameters.IncludeAnchorTabLocations = includeAnchorTabLocations;
            }
            if (includeIncludeMetadata)
            {
                config.QueryParameters.IncludeMetadata = includeMetadata;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Tabs>(response)!;
    }

    public Tabs RecipientsPutTemplateRecipientTabs(string accountId, string recipientId, string templateId, TemplateTabs requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Recipients[recipientId].Tabs;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TemplateTabs>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Tabs>(response)!;
    }

    public Tabs RecipientsPostTemplateRecipientTabs(string accountId, string recipientId, string templateId, TemplateTabs requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Recipients[recipientId].Tabs;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TemplateTabs>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Tabs>(response)!;
    }

    public Tabs RecipientsDeleteTemplateRecipientTabs(string accountId, string recipientId, string templateId, TemplateTabs requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Recipients[recipientId].Tabs;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TemplateTabs>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Tabs>(response)!;
    }

    public TemplateDocumentVisibilityList RecipientsPutTemplateRecipientsDocumentVisibility(string accountId, string templateId, TemplateDocumentVisibilityList requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Recipients.Document_visibility;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TemplateDocumentVisibilityList>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<TemplateDocumentVisibilityList>(response)!;
    }

    public DocumentHtmlDefinitions ResponsiveHtmlPostTemplateResponsiveHtmlPreview(string accountId, string templateId, DocumentHtmlDefinition requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Responsive_html_preview;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.DocumentHtmlDefinition>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DocumentHtmlDefinitions>(response)!;
    }

    public ViewUrl ViewsPostTemplateEditView(string accountId, string templateId, TemplateViewRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Views.Edit;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TemplateViewRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ViewUrl>(response)!;
    }

    public ViewUrl ViewsPostTemplateRecipientPreview(string accountId, string templateId, RecipientPreviewRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Views.Recipient_preview;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.RecipientPreviewRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ViewUrl>(response)!;
    }

    public Workflow TemplateWorkflowDefinitionGetTemplateWork_a5e5af69(string accountId, string templateId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Workflow;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Workflow>(response)!;
    }

    public Workflow TemplateWorkflowDefinitionPutTemplateWork_f659df72(string accountId, string templateId, Workflow requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Workflow;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.Workflow>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Workflow>(response)!;
    }

    public byte[] TemplateWorkflowDefinitionDeleteTemplateW_3824e45a(string accountId, string templateId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Workflow;
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public ScheduledSending TemplateWorkflowScheduledSendingGetTempla_7ca3441d(string accountId, string templateId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Workflow.ScheduledSending;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ScheduledSending>(response)!;
    }

    public ScheduledSending TemplateWorkflowScheduledSendingPutTempla_3677d893(string accountId, string templateId, ScheduledSending requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Workflow.ScheduledSending;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.ScheduledSending>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ScheduledSending>(response)!;
    }

    public byte[] TemplateWorkflowScheduledSendingDeleteTem_c0241f55(string accountId, string templateId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Workflow.ScheduledSending;
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public WorkflowStep TemplateWorkflowStepPostTemplateWorkflowS_2e2b4e6f(string accountId, string templateId, WorkflowStep requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Workflow.Steps;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.WorkflowStep>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<WorkflowStep>(response)!;
    }

    public WorkflowStep TemplateWorkflowStepGetTemplateWorkflowSt_2c37995b(string accountId, string templateId, string workflowStepId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Workflow.Steps[workflowStepId];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<WorkflowStep>(response)!;
    }

    public WorkflowStep TemplateWorkflowStepPutTemplateWorkflowSt_00ec792c(string accountId, string templateId, string workflowStepId, WorkflowStep requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Workflow.Steps[workflowStepId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.WorkflowStep>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<WorkflowStep>(response)!;
    }

    public byte[] TemplateWorkflowStepDeleteTemplateWorkflo_21662918(string accountId, string templateId, string workflowStepId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Workflow.Steps[workflowStepId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public DelayedRouting TemplateWorkflowDelayedRoutingGetTemplate_6bb9d0ab(string accountId, string templateId, string workflowStepId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Workflow.Steps[workflowStepId].DelayedRouting;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DelayedRouting>(response)!;
    }

    public DelayedRouting TemplateWorkflowDelayedRoutingPutTemplate_844b84c8(string accountId, string templateId, string workflowStepId, DelayedRouting requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Workflow.Steps[workflowStepId].DelayedRouting;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.DelayedRouting>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DelayedRouting>(response)!;
    }

    public byte[] TemplateWorkflowDelayedRoutingDeleteTempl_582fffd9(string accountId, string templateId, string workflowStepId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates[templateId].Workflow.Steps[workflowStepId].DelayedRouting;
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public TemplateAutoMatchList TemplatesAutoMatchPutTemplates(string accountId, TemplateAutoMatchList requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Templates.Auto_match;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.TemplateAutoMatchList>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<TemplateAutoMatchList>(response)!;
    }

    public FileTypeList UnsupportedFileTypesGetUnsupportedFileTypes(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Unsupported_file_types;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<FileTypeList>(response)!;
    }

    public UserInformationList UsersGetUsers(string accountId, string additionalInfo = "", bool includeAdditionalInfo = false, string alternateAdminsOnly = "", bool includeAlternateAdminsOnly = false, string count = "", bool includeCount = false, string domainUsersOnly = "", bool includeDomainUsersOnly = false, string email = "", bool includeEmail = false, string emailSubstring = "", bool includeEmailSubstring = false, string groupId = "", bool includeGroupId = false, string includeLicense = "", bool includeIncludeLicense = false, string includeUsersettingsForCsv = "", bool includeIncludeUsersettingsForCsv = false, string loginStatus = "", bool includeLoginStatus = false, string notGroupId = "", bool includeNotGroupId = false, string startPosition = "", bool includeStartPosition = false, string status = "", bool includeStatus = false, string userNameSubstring = "", bool includeUserNameSubstring = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeAdditionalInfo)
            {
                config.QueryParameters.AdditionalInfo = additionalInfo;
            }
            if (includeAlternateAdminsOnly)
            {
                config.QueryParameters.AlternateAdminsOnly = alternateAdminsOnly;
            }
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeDomainUsersOnly)
            {
                config.QueryParameters.DomainUsersOnly = domainUsersOnly;
            }
            if (includeEmail)
            {
                config.QueryParameters.Email = email;
            }
            if (includeEmailSubstring)
            {
                config.QueryParameters.EmailSubstring = emailSubstring;
            }
            if (includeGroupId)
            {
                config.QueryParameters.GroupId = groupId;
            }
            if (includeIncludeLicense)
            {
                config.QueryParameters.IncludeLicense = includeLicense;
            }
            if (includeIncludeUsersettingsForCsv)
            {
                config.QueryParameters.IncludeUsersettingsForCsv = includeUsersettingsForCsv;
            }
            if (includeLoginStatus)
            {
                config.QueryParameters.LoginStatus = loginStatus;
            }
            if (includeNotGroupId)
            {
                config.QueryParameters.NotGroupId = notGroupId;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
            if (includeStatus)
            {
                config.QueryParameters.Status = status;
            }
            if (includeUserNameSubstring)
            {
                config.QueryParameters.UserNameSubstring = userNameSubstring;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserInformationList>(response)!;
    }

    public UserInformationList UsersPutUsers(string accountId, UserInformationList requestBody, string allowAllLanguages = "", bool includeAllowAllLanguages = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.UserInformationList>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserInformationList>(response)!;
    }

    public NewUsersSummary UsersPostUsers(string accountId, NewUsersDefinition requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.NewUsersDefinition>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<NewUsersSummary>(response)!;
    }

    public UsersResponse UsersDeleteUsers(string accountId, UserInfoList requestBody, string delete = "", bool includeDelete = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.UserInfoList>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UsersResponse>(response)!;
    }

    public UserInformation UserGetUser(string accountId, string userId, string additionalInfo = "", bool includeAdditionalInfo = false, string email = "", bool includeEmail = false, string includeLicense = "", bool includeIncludeLicense = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeAdditionalInfo)
            {
                config.QueryParameters.AdditionalInfo = additionalInfo;
            }
            if (includeEmail)
            {
                config.QueryParameters.Email = email;
            }
            if (includeIncludeLicense)
            {
                config.QueryParameters.IncludeLicense = includeLicense;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserInformation>(response)!;
    }

    public UserInformation UserPutUser(string accountId, string userId, UserInformation requestBody, string allowAllLanguages = "", bool includeAllowAllLanguages = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.UserInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserInformation>(response)!;
    }

    public UserAuthorization UserAuthorizationCreateUserAuthorization(string accountId, string userId, UserAuthorizationCreateRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Authorization;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.UserAuthorizationCreateRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserAuthorization>(response)!;
    }

    public UserAuthorization UserAuthorizationGetUserAuthorization(string accountId, string authorizationId, string userId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Authorization[authorizationId];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserAuthorization>(response)!;
    }

    public UserAuthorization UserAuthorizationUpdateUserAuthorization(string accountId, string authorizationId, string userId, UserAuthorizationUpdateRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Authorization[authorizationId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.UserAuthorizationUpdateRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserAuthorization>(response)!;
    }

    public byte[] UserAuthorizationDeleteUserAuthorization(string accountId, string authorizationId, string userId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Authorization[authorizationId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public UserAuthorizations UserAuthorizationsGetPrincipalUserAuthorizations(string accountId, string userId, string activeOnly = "", bool includeActiveOnly = false, string count = "", bool includeCount = false, string emailSubstring = "", bool includeEmailSubstring = false, string includeClosedUsers = "", bool includeIncludeClosedUsers = false, string permissions = "", bool includePermissions = false, string startPosition = "", bool includeStartPosition = false, string userNameSubstring = "", bool includeUserNameSubstring = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Authorizations;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeActiveOnly)
            {
                config.QueryParameters.ActiveOnly = activeOnly;
            }
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeEmailSubstring)
            {
                config.QueryParameters.EmailSubstring = emailSubstring;
            }
            if (includeIncludeClosedUsers)
            {
                config.QueryParameters.IncludeClosedUsers = includeClosedUsers;
            }
            if (includePermissions)
            {
                config.QueryParameters.Permissions = permissions;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
            if (includeUserNameSubstring)
            {
                config.QueryParameters.UserNameSubstring = userNameSubstring;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserAuthorizations>(response)!;
    }

    public UserAuthorizationsResponse UserAuthorizationsPostUserAuthorizations(string accountId, string userId, UserAuthorizationsRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Authorizations;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.UserAuthorizationsRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserAuthorizationsResponse>(response)!;
    }

    public UserAuthorizationsDeleteResponse UserAuthorizationsDeleteUserAuthorizations(string accountId, string userId, UserAuthorizationsDeleteRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Authorizations;
        var response = requestBuilder.DeleteAsUserAuthorizationsDeleteResponseAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.UserAuthorizationsDeleteRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserAuthorizationsDeleteResponse>(response)!;
    }

    public UserAuthorizations UserAgentAuthorizationsGetAgentUserAuthorizations(string accountId, string userId, string activeOnly = "", bool includeActiveOnly = false, string count = "", bool includeCount = false, string emailSubstring = "", bool includeEmailSubstring = false, string includeClosedUsers = "", bool includeIncludeClosedUsers = false, string permissions = "", bool includePermissions = false, string startPosition = "", bool includeStartPosition = false, string userNameSubstring = "", bool includeUserNameSubstring = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Authorizations.Agent;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeActiveOnly)
            {
                config.QueryParameters.ActiveOnly = activeOnly;
            }
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeEmailSubstring)
            {
                config.QueryParameters.EmailSubstring = emailSubstring;
            }
            if (includeIncludeClosedUsers)
            {
                config.QueryParameters.IncludeClosedUsers = includeClosedUsers;
            }
            if (includePermissions)
            {
                config.QueryParameters.Permissions = permissions;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
            if (includeUserNameSubstring)
            {
                config.QueryParameters.UserNameSubstring = userNameSubstring;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserAuthorizations>(response)!;
    }

    public CloudStorageProviders CloudStorageGetCloudStorageProviders(string accountId, string userId, string redirectUrl = "", bool includeRedirectUrl = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Cloud_storage;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeRedirectUrl)
            {
                config.QueryParameters.RedirectUrl = redirectUrl;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<CloudStorageProviders>(response)!;
    }

    public CloudStorageProviders CloudStoragePostCloudStorage(string accountId, string userId, CloudStorageProviders requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Cloud_storage;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.CloudStorageProviders>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<CloudStorageProviders>(response)!;
    }

    public CloudStorageProviders CloudStorageDeleteCloudStorageProviders(string accountId, string userId, CloudStorageProviders requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Cloud_storage;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.CloudStorageProviders>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<CloudStorageProviders>(response)!;
    }

    public CloudStorageProviders CloudStorageGetCloudStorage(string accountId, string serviceId, string userId, string redirectUrl = "", bool includeRedirectUrl = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Cloud_storage[serviceId];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeRedirectUrl)
            {
                config.QueryParameters.RedirectUrl = redirectUrl;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<CloudStorageProviders>(response)!;
    }

    public CloudStorageProviders CloudStorageDeleteCloudStorage(string accountId, string serviceId, string userId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Cloud_storage[serviceId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<CloudStorageProviders>(response)!;
    }

    public ExternalFolder CloudStorageFolderGetCloudStorageFolderAll(string accountId, string serviceId, string userId, string cloudStorageFolderPath = "", bool includeCloudStorageFolderPath = false, string count = "", bool includeCount = false, string order = "", bool includeOrder = false, string orderBy = "", bool includeOrderBy = false, string searchText = "", bool includeSearchText = false, string startPosition = "", bool includeStartPosition = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Cloud_storage[serviceId].Folders;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCloudStorageFolderPath)
            {
                config.QueryParameters.CloudStorageFolderPath = cloudStorageFolderPath;
            }
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeOrder)
            {
                config.QueryParameters.Order = order;
            }
            if (includeOrderBy)
            {
                config.QueryParameters.OrderBy = orderBy;
            }
            if (includeSearchText)
            {
                config.QueryParameters.SearchText = searchText;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ExternalFolder>(response)!;
    }

    public ExternalFolder CloudStorageFolderGetCloudStorageFolder(string accountId, string folderId, string serviceId, string userId, string cloudStorageFolderPath = "", bool includeCloudStorageFolderPath = false, string cloudStorageFolderidPlain = "", bool includeCloudStorageFolderidPlain = false, string count = "", bool includeCount = false, string order = "", bool includeOrder = false, string orderBy = "", bool includeOrderBy = false, string searchText = "", bool includeSearchText = false, string skyDriveSkipToken = "", bool includeSkyDriveSkipToken = false, string startPosition = "", bool includeStartPosition = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Cloud_storage[serviceId].Folders[folderId];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCloudStorageFolderPath)
            {
                config.QueryParameters.CloudStorageFolderPath = cloudStorageFolderPath;
            }
            if (includeCloudStorageFolderidPlain)
            {
                config.QueryParameters.CloudStorageFolderidPlain = cloudStorageFolderidPlain;
            }
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeOrder)
            {
                config.QueryParameters.Order = order;
            }
            if (includeOrderBy)
            {
                config.QueryParameters.OrderBy = orderBy;
            }
            if (includeSearchText)
            {
                config.QueryParameters.SearchText = searchText;
            }
            if (includeSkyDriveSkipToken)
            {
                config.QueryParameters.SkyDriveSkipToken = skyDriveSkipToken;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ExternalFolder>(response)!;
    }

    public CustomSettingsInformation UserCustomSettingsGetCustomSettings(string accountId, string userId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Custom_settings;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<CustomSettingsInformation>(response)!;
    }

    public CustomSettingsInformation UserCustomSettingsPutCustomSettings(string accountId, string userId, CustomSettingsInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Custom_settings;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.CustomSettingsInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<CustomSettingsInformation>(response)!;
    }

    public CustomSettingsInformation UserCustomSettingsDeleteCustomSettings(string accountId, string userId, CustomSettingsInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Custom_settings;
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.CustomSettingsInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<CustomSettingsInformation>(response)!;
    }

    public UserProfile UserProfileGetProfile(string accountId, string userId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Profile;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserProfile>(response)!;
    }

    public byte[] UserProfilePutProfile(string accountId, string userId, UserProfile requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Profile;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.UserProfile>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public byte[] UserProfileImageGetUserProfileImage(string accountId, string userId, string encoding = "", bool includeEncoding = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Profile.Image;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeEncoding)
            {
                config.QueryParameters.Encoding = encoding;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public byte[] UserProfileImagePutUserProfileImage(string accountId, string userId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Profile.Image;
        var response = requestBuilder.PutAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public byte[] UserProfileImageDeleteUserProfileImage(string accountId, string userId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Profile.Image;
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public UserSettingsInformation UserSettingsGetUserSettings(string accountId, string userId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Settings;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserSettingsInformation>(response)!;
    }

    public byte[] UserSettingsPutUserSettings(string accountId, string userId, UserSettingsInformation requestBody, string allowAllLanguages = "", bool includeAllowAllLanguages = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Settings;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.UserSettingsInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public UserSignaturesInformation UserSignaturesGetUserSignatures(string accountId, string userId, string stampType = "", bool includeStampType = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Signatures;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeStampType)
            {
                config.QueryParameters.StampType = stampType;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserSignaturesInformation>(response)!;
    }

    public UserSignaturesInformation UserSignaturesPutUserSignature(string accountId, string userId, UserSignaturesInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Signatures;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.UserSignaturesInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserSignaturesInformation>(response)!;
    }

    public UserSignaturesInformation UserSignaturesPostUserSignatures(string accountId, string userId, UserSignaturesInformation requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Signatures;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.UserSignaturesInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserSignaturesInformation>(response)!;
    }

    public UserSignature UserSignaturesGetUserSignature(string accountId, string signatureId, string userId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Signatures[signatureId];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserSignature>(response)!;
    }

    public UserSignature UserSignaturesPutUserSignatureById(string accountId, string signatureId, string userId, UserSignatureDefinition requestBody, string closeExistingSignature = "", bool includeCloseExistingSignature = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Signatures[signatureId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.UserSignatureDefinition>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserSignature>(response)!;
    }

    public byte[] UserSignaturesDeleteUserSignature(string accountId, string signatureId, string userId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Signatures[signatureId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public byte[] UserSignaturesGetUserSignatureImage(string accountId, string imageType, string signatureId, string userId, string includeChrome = "", bool includeIncludeChrome = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Signatures[signatureId][imageType];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeIncludeChrome)
            {
                config.QueryParameters.IncludeChrome = includeChrome;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public UserSignature UserSignaturesPutUserSignatureImage(string accountId, string imageType, string signatureId, string userId, byte[] requestBody, string transparentPng = "", bool includeTransparentPng = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Signatures[signatureId][imageType];
        var response = requestBuilder.PutAsync(new MemoryStream(requestBody ?? Array.Empty<byte>())).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserSignature>(response)!;
    }

    public UserSignature UserSignaturesDeleteUserSignatureImage(string accountId, string imageType, string signatureId, string userId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Users[userId].Signatures[signatureId][imageType];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserSignature>(response)!;
    }

    public EnvelopeViews ViewsPostAccountConsoleView(string accountId, ConsoleViewRequest requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Views.Console;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.ConsoleViewRequest>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<EnvelopeViews>(response)!;
    }

    public Watermark WatermarkGetWatermark(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Watermark;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Watermark>(response)!;
    }

    public Watermark WatermarkPutWatermark(string accountId, Watermark requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Watermark;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.Watermark>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Watermark>(response)!;
    }

    public Watermark WatermarkPreviewPutWatermarkPreview(string accountId, Watermark requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Watermark.Preview;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.Watermark>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Watermark>(response)!;
    }

    public WorkspaceList WorkspaceGetWorkspaces(string accountId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Workspaces;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<WorkspaceList>(response)!;
    }

    public Workspace WorkspacePostWorkspace(string accountId, Workspace requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Workspaces;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.Workspace>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Workspace>(response)!;
    }

    public Workspace WorkspaceGetWorkspace(string accountId, string workspaceId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Workspaces[workspaceId];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Workspace>(response)!;
    }

    public Workspace WorkspacePutWorkspace(string accountId, string workspaceId, Workspace requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Workspaces[workspaceId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.Workspace>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Workspace>(response)!;
    }

    public Workspace WorkspaceDeleteWorkspace(string accountId, string workspaceId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Workspaces[workspaceId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Workspace>(response)!;
    }

    public WorkspaceFolderContents WorkspaceFolderGetWorkspaceFolder(string accountId, string folderId, string workspaceId, string count = "", bool includeCount = false, string includeFiles = "", bool includeIncludeFiles = false, string includeSubFolders = "", bool includeIncludeSubFolders = false, string includeThumbnails = "", bool includeIncludeThumbnails = false, string includeUserDetail = "", bool includeIncludeUserDetail = false, string startPosition = "", bool includeStartPosition = false, string workspaceUserId = "", bool includeWorkspaceUserId = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Workspaces[workspaceId].Folders[folderId];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeIncludeFiles)
            {
                config.QueryParameters.IncludeFiles = includeFiles;
            }
            if (includeIncludeSubFolders)
            {
                config.QueryParameters.IncludeSubFolders = includeSubFolders;
            }
            if (includeIncludeThumbnails)
            {
                config.QueryParameters.IncludeThumbnails = includeThumbnails;
            }
            if (includeIncludeUserDetail)
            {
                config.QueryParameters.IncludeUserDetail = includeUserDetail;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
            if (includeWorkspaceUserId)
            {
                config.QueryParameters.WorkspaceUserId = workspaceUserId;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<WorkspaceFolderContents>(response)!;
    }

    public byte[] WorkspaceFolderDeleteWorkspaceItems(string accountId, string folderId, string workspaceId, WorkspaceItemList requestBody)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Workspaces[workspaceId].Folders[folderId];
        var response = requestBuilder.DeleteAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.WorkspaceItemList>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public WorkspaceItem WorkspaceFilePostWorkspaceFiles(string accountId, string folderId, string workspaceId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Workspaces[workspaceId].Folders[folderId].Files;
        var response = requestBuilder.PostAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<WorkspaceItem>(response)!;
    }

    public byte[] WorkspaceFileGetWorkspaceFile(string accountId, string fileId, string folderId, string workspaceId, string isDownload = "", bool includeIsDownload = false, string pdfVersion = "", bool includePdfVersion = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Workspaces[workspaceId].Folders[folderId].Files[fileId];
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeIsDownload)
            {
                config.QueryParameters.IsDownload = isDownload;
            }
            if (includePdfVersion)
            {
                config.QueryParameters.PdfVersion = pdfVersion;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public WorkspaceItem WorkspaceFilePutWorkspaceFile(string accountId, string fileId, string folderId, string workspaceId)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Workspaces[workspaceId].Folders[folderId].Files[fileId];
        var response = requestBuilder.PutAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<WorkspaceItem>(response)!;
    }

    public PageImages WorkspaceFilePagesGetWorkspaceFilePages(string accountId, string fileId, string folderId, string workspaceId, string count = "", bool includeCount = false, string dpi = "", bool includeDpi = false, string maxHeight = "", bool includeMaxHeight = false, string maxWidth = "", bool includeMaxWidth = false, string startPosition = "", bool includeStartPosition = false)
    {
        var requestBuilder = _client.V21.Accounts[accountId].Workspaces[workspaceId].Folders[folderId].Files[fileId].Pages;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeDpi)
            {
                config.QueryParameters.Dpi = dpi;
            }
            if (includeMaxHeight)
            {
                config.QueryParameters.MaxHeight = maxHeight;
            }
            if (includeMaxWidth)
            {
                config.QueryParameters.MaxWidth = maxWidth;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<PageImages>(response)!;
    }

    public ProvisioningInformation AccountsGetProvisioning()
    {
        var requestBuilder = _client.V21.Accounts.Provisioning;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ProvisioningInformation>(response)!;
    }

    public BillingPlansResponse BillingPlansGetBillingPlans()
    {
        var requestBuilder = _client.V21.Billing_plans;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BillingPlansResponse>(response)!;
    }

    public BillingPlanResponse BillingPlansGetBillingPlan(string billingPlanId)
    {
        var requestBuilder = _client.V21.Billing_plans[billingPlanId];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<BillingPlanResponse>(response)!;
    }

    public NotaryResult NotaryGetNotary(string includeJurisdictions = "", bool includeIncludeJurisdictions = false)
    {
        var requestBuilder = _client.V21.Current_user.Notary;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeIncludeJurisdictions)
            {
                config.QueryParameters.IncludeJurisdictions = includeJurisdictions;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<NotaryResult>(response)!;
    }

    public Notary NotaryPutNotary(Notary requestBody)
    {
        var requestBuilder = _client.V21.Current_user.Notary;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.Notary>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Notary>(response)!;
    }

    public Notary NotaryPostNotary(Notary requestBody)
    {
        var requestBuilder = _client.V21.Current_user.Notary;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.Notary>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<Notary>(response)!;
    }

    public NotaryJournalList NotaryJournalsGetNotaryJournals(string count = "", bool includeCount = false, string searchText = "", bool includeSearchText = false, string startPosition = "", bool includeStartPosition = false)
    {
        var requestBuilder = _client.V21.Current_user.Notary.Journals;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeCount)
            {
                config.QueryParameters.Count = count;
            }
            if (includeSearchText)
            {
                config.QueryParameters.SearchText = searchText;
            }
            if (includeStartPosition)
            {
                config.QueryParameters.StartPosition = startPosition;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<NotaryJournalList>(response)!;
    }

    public NotaryJurisdictionList NotaryJurisdictionsGetNotaryJurisdictions()
    {
        var requestBuilder = _client.V21.Current_user.Notary.Jurisdictions;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<NotaryJurisdictionList>(response)!;
    }

    public NotaryJurisdiction NotaryJurisdictionsPostNotaryJurisdictions(NotaryJurisdiction requestBody)
    {
        var requestBuilder = _client.V21.Current_user.Notary.Jurisdictions;
        var response = requestBuilder.PostAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.NotaryJurisdiction>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<NotaryJurisdiction>(response)!;
    }

    public NotaryJurisdiction NotaryJurisdictionsGetNotaryJurisdiction(string jurisdictionId)
    {
        var requestBuilder = _client.V21.Current_user.Notary.Jurisdictions[jurisdictionId];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<NotaryJurisdiction>(response)!;
    }

    public NotaryJurisdiction NotaryJurisdictionsPutNotaryJurisdiction(string jurisdictionId, NotaryJurisdiction requestBody)
    {
        var requestBuilder = _client.V21.Current_user.Notary.Jurisdictions[jurisdictionId];
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.NotaryJurisdiction>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<NotaryJurisdiction>(response)!;
    }

    public byte[] NotaryJurisdictionsDeleteNotaryJurisdiction(string jurisdictionId)
    {
        var requestBuilder = _client.V21.Current_user.Notary.Jurisdictions[jurisdictionId];
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public UserPasswordRules PasswordRulesGetPasswordRules()
    {
        var requestBuilder = _client.V21.Current_user.Password_rules;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<UserPasswordRules>(response)!;
    }

    public ApiRequestLogsResult APIRequestLogGetRequestLogs(string encoding = "", bool includeEncoding = false)
    {
        var requestBuilder = _client.V21.Diagnostics.Request_logs;
        var response = requestBuilder.GetAsync(config =>
        {
            if (includeEncoding)
            {
                config.QueryParameters.Encoding = encoding;
            }
        }).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<ApiRequestLogsResult>(response)!;
    }

    public byte[] APIRequestLogDeleteRequestLogs()
    {
        var requestBuilder = _client.V21.Diagnostics.Request_logs;
        var response = requestBuilder.DeleteAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.ReadAllBytes(response);
    }

    public string APIRequestLogGetRequestLog(string requestLogId)
    {
        var requestBuilder = _client.V21.Diagnostics.Request_logs[requestLogId];
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return response ?? string.Empty;
    }

    public DiagnosticsSettingsInformation APIRequestLogGetRequestLogSettings()
    {
        var requestBuilder = _client.V21.Diagnostics.Settings;
        var response = requestBuilder.GetAsync().GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DiagnosticsSettingsInformation>(response)!;
    }

    public DiagnosticsSettingsInformation APIRequestLogPutRequestLogSettings(DiagnosticsSettingsInformation requestBody)
    {
        var requestBuilder = _client.V21.Diagnostics.Settings;
        var response = requestBuilder.PutAsync(GeneratedModelMapper.Convert<DocuSignKiota.Client.Models.DiagnosticsSettingsInformation>(requestBody)!).GetAwaiter().GetResult();
        return GeneratedModelMapper.Convert<DiagnosticsSettingsInformation>(response)!;
    }

}

[OSInterface(Description = "Generated OutSystems wrapper for DocuSign eSignature API.", Name = "DocuSignActionsGenerated")]
public interface IDocuSignActionsGenerated
{
    [OSAction(Description = "Retrieves the available REST API versions.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ServiceInformation ServiceInformationGetServiceInformation();

    [OSAction(Description = "Lists resources for REST version specified", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ResourceInformation ServiceInformationGetResourceInformation();

    [OSAction(Description = "Creates new accounts.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    NewAccountSummary AccountsPostAccounts([OSParameter(Description = "The JSON request body payload.")] NewAccountDefinition requestBody);

    [OSAction(Description = "Retrieves the account information for the specified account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountInformation AccountsGetAccount([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "When **true,** includes account settings in the response. The default value is **false.**")] string includeAccountSettings = "", [OSParameter(Description = "Set to true to send the include_account_settings query parameter to the API.")] bool includeIncludeAccountSettings = false, [OSParameter(Description = "The include_trial_eligibility query parameter.")] string includeTrialEligibility = "", [OSParameter(Description = "Set to true to send the include_trial_eligibility query parameter to the API.")] bool includeIncludeTrialEligibility = false);

    [OSAction(Description = "Deletes the specified account.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] AccountsDeleteAccount([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The redact_user_data query parameter.")] string redactUserData = "", [OSParameter(Description = "Set to true to send the redact_user_data query parameter to the API.")] bool includeRedactUserData = false);

    [OSAction(Description = "Gets list of recurring and usage charges for the account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BillingChargeResponse BillingChargesGetAccountBillingCharges([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "Specifies which billing charges to return. Valid values are: * envelopes * seats")] string includeCharges = "", [OSParameter(Description = "Set to true to send the include_charges query parameter to the API.")] bool includeIncludeCharges = false);

    [OSAction(Description = "Get a List of Billing Invoices", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BillingInvoicesResponse BillingInvoicesGetBillingInvoices([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "Specifies the date/time of the earliest invoice in the account to retrieve.")] string fromDate = "", [OSParameter(Description = "Set to true to send the from_date query parameter to the API.")] bool includeFromDate = false, [OSParameter(Description = "Specifies the date/time of the latest invoice in the account to retrieve.")] string toDate = "", [OSParameter(Description = "Set to true to send the to_date query parameter to the API.")] bool includeToDate = false);

    [OSAction(Description = "Retrieves a billing invoice.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BillingInvoice BillingInvoicesGetBillingInvoice([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the invoice.")] string invoiceId);

    [OSAction(Description = "Get a list of past due invoices.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BillingInvoicesSummary BillingInvoicesGetBillingInvoicesPastDue([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Gets payment information for one or more payments.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BillingPaymentsResponse BillingPaymentsGetPaymentList([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "Specifies the date/time of the earliest payment in the account to retrieve.")] string fromDate = "", [OSParameter(Description = "Set to true to send the from_date query parameter to the API.")] bool includeFromDate = false, [OSParameter(Description = "Specifies the date/time of the latest payment in the account to retrieve.")] string toDate = "", [OSParameter(Description = "Set to true to send the to_date query parameter to the API.")] bool includeToDate = false);

    [OSAction(Description = "Posts a payment to a past due invoice.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BillingPaymentResponse BillingPaymentsPostPayment([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] BillingPaymentRequest requestBody);

    [OSAction(Description = "Gets billing payment information for a specific payment.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BillingPaymentItem BillingPaymentsGetPayment([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the payment.")] string paymentId);

    [OSAction(Description = "Get Account Billing Plan", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountBillingPlanResponse BillingPlanGetBillingPlan([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "When **true,** payment information including credit card information will show in the return.")] string includeCreditCardInformation = "", [OSParameter(Description = "Set to true to send the include_credit_card_information query parameter to the API.")] bool includeIncludeCreditCardInformation = false, [OSParameter(Description = "The include_downgrade_information query parameter.")] string includeDowngradeInformation = "", [OSParameter(Description = "Set to true to send the include_downgrade_information query parameter to the API.")] bool includeIncludeDowngradeInformation = false, [OSParameter(Description = "When **true,** the `canUpgrade` and `renewalStatus` properties are included the response and an array of `supportedCountries` is added to the `billingAddress` information.")] string includeMetadata = "", [OSParameter(Description = "Set to true to send the include_metadata query parameter to the API.")] bool includeIncludeMetadata = false, [OSParameter(Description = "When **true,** excludes successor information from the response.")] string includeSuccessorPlans = "", [OSParameter(Description = "Set to true to send the include_successor_plans query parameter to the API.")] bool includeIncludeSuccessorPlans = false, [OSParameter(Description = "The include_tax_exempt_id query parameter.")] string includeTaxExemptId = "", [OSParameter(Description = "Set to true to send the include_tax_exempt_id query parameter to the API.")] bool includeIncludeTaxExemptId = false);

    [OSAction(Description = "Updates an account billing plan.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BillingPlanUpdateResponse BillingPlanPutBillingPlan([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] BillingPlanInformation requestBody, [OSParameter(Description = "When **true,** updates the account using a preview billing plan.")] string previewBillingPlan = "", [OSParameter(Description = "Set to true to send the preview_billing_plan query parameter to the API.")] bool includePreviewBillingPlan = false);

    [OSAction(Description = "Get credit card information", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    CreditCardInformation BillingPlanGetCreditCardInfo([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Returns downgrade plan information for the specified account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DowngradRequestBillingInfoResponse BillingPlanGetDowngradeRequestBillingInfo([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Queues downgrade billing plan request for an account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DowngradePlanUpdateResponse BillingPlanPutDowngradeAccountBillingPlan([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] DowngradeBillingPlanInformation requestBody);

    [OSAction(Description = "Reserved: Purchase additional envelopes.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] PurchasedEnvelopesPutPurchasedEnvelopes([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] PurchasedEnvelopesInformation requestBody);

    [OSAction(Description = "Gets a list of brands.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountBrands BrandsGetBrands([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "When **true,** excludes distributor brand information from the response set.")] string excludeDistributorBrand = "", [OSParameter(Description = "Set to true to send the exclude_distributor_brand query parameter to the API.")] bool includeExcludeDistributorBrand = false, [OSParameter(Description = "When **true,** returns the logos associated with the brand.")] string includeLogos = "", [OSParameter(Description = "Set to true to send the include_logos query parameter to the API.")] bool includeIncludeLogos = false);

    [OSAction(Description = "Creates one or more brand profiles for an account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountBrands BrandsPostBrands([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] Brand requestBody);

    [OSAction(Description = "Deletes one or more brand profiles.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountBrands BrandsDeleteBrands([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] BrandsRequest requestBody);

    [OSAction(Description = "Gets information about a brand.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Brand BrandGetBrand([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the brand.")] string brandId, [OSParameter(Description = "When **true,** the landing pages and links associated with the brand are included in the response.")] string includeExternalReferences = "", [OSParameter(Description = "Set to true to send the include_external_references query parameter to the API.")] bool includeIncludeExternalReferences = false, [OSParameter(Description = "When **true,** the URIs for the logos associated with the brand are included in the response.")] string includeLogos = "", [OSParameter(Description = "Set to true to send the include_logos query parameter to the API.")] bool includeIncludeLogos = false);

    [OSAction(Description = "Updates an existing brand.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Brand BrandPutBrand([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the brand.")] string brandId, [OSParameter(Description = "The JSON request body payload.")] Brand requestBody, [OSParameter(Description = "When **true,** replaces the brand instead of updating it. The only unchanged value is the brand ID. The request body must be XML. The default value is **false.**")] string replaceBrand = "", [OSParameter(Description = "Set to true to send the replace_brand query parameter to the API.")] bool includeReplaceBrand = false);

    [OSAction(Description = "Deletes a brand.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] BrandDeleteBrand([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the brand.")] string brandId);

    [OSAction(Description = "Exports a brand.")]
    void BrandExportGetBrandExportFile([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the brand.")] string brandId);

    [OSAction(Description = "Gets a brand logo.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] BrandLogoGetBrandLogo([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the brand.")] string brandId, [OSParameter(Description = "The type of logo. Valid values are: - `primary` - `secondary` - `email`")] string logoType);

    [OSAction(Description = "Updates a brand logo.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] BrandLogoPutBrandLogo([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the brand.")] string brandId, [OSParameter(Description = "The type of logo. Valid values are: - `primary` - `secondary` - `email`")] string logoType, [OSParameter(Description = "The binary request body payload.")] byte[] requestBody);

    [OSAction(Description = "Deletes a brand logo.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] BrandLogoDeleteBrandLogo([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the brand.")] string brandId, [OSParameter(Description = "The type of logo. Valid values are: - `primary` - `secondary` - `email`")] string logoType);

    [OSAction(Description = "Returns metadata about the branding resources for an account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BrandResourcesList BrandResourcesGetBrandResourcesList([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the brand.")] string brandId);

    [OSAction(Description = "Returns a branding resource file.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] BrandResourcesGetBrandResources([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the brand.")] string brandId, [OSParameter(Description = "The type of brand resource file to return. Valid values are: - `sending` - `signing` - `email` - `signing_captive`")] string resourceContentType, [OSParameter(Description = "The ISO 3166-1 alpha-2 codes for the languages that the brand supports.")] string langcode = "", [OSParameter(Description = "Set to true to send the langcode query parameter to the API.")] bool includeLangcode = false, [OSParameter(Description = "Specifies which resource file data to return. When **true,** only the master resource file is returned. When **false,** only the elements that you modified are returned.")] string returnMaster = "", [OSParameter(Description = "Set to true to send the return_master query parameter to the API.")] bool includeReturnMaster = false);

    [OSAction(Description = "Updates a branding resource file.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BrandResources BrandResourcesPutBrandResources([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the brand.")] string brandId, [OSParameter(Description = "The type of brand resource file that you are updating. Valid values are: - `sending` - `signing` - `email` - `signing_captive`")] string resourceContentType, [OSParameter(Description = "The JSON request body payload.")] BrandResourcesPutBrandResourcesRequest requestBody);

    [OSAction(Description = "Returns a list of bulk send batch summaries.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BulkSendBatchSummaries BulkSendV2BatchGetBulkSendBatches([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "A comma-separated list of batch IDs to query.")] string batchIds = "", [OSParameter(Description = "Set to true to send the batch_ids query parameter to the API.")] bool includeBatchIds = false, [OSParameter(Description = "The maximum number of results to return. Use `start_position` to specify the number of results to skip. Valid values: `1` to `100`<br> Default: `100`")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "The start date for a date range in UTC DateTime format.")] string fromDate = "", [OSParameter(Description = "Set to true to send the from_date query parameter to the API.")] bool includeFromDate = false, [OSParameter(Description = "Use this parameter to search for specific text.")] string searchText = "", [OSParameter(Description = "Set to true to send the search_text query parameter to the API.")] bool includeSearchText = false, [OSParameter(Description = "The zero-based index of the result from which to start returning results. Use with `count` to limit the number of results. The default value is `0`.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false, [OSParameter(Description = "The kind of results to collect. Must be one of: - all - failed - sent - queued")] string status = "", [OSParameter(Description = "Set to true to send the status query parameter to the API.")] bool includeStatus = false, [OSParameter(Description = "The end of a search date range in UTC DateTime format. When you use this parameter, only templates created up to this date and time are returned. **Note:** If this property is null, the value defaults to the current date.")] string toDate = "", [OSParameter(Description = "Set to true to send the to_date query parameter to the API.")] bool includeToDate = false, [OSParameter(Description = "The user_id query parameter.")] string userId = "", [OSParameter(Description = "Set to true to send the user_id query parameter to the API.")] bool includeUserId = false);

    [OSAction(Description = "Gets the status of a specific bulk send batch.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BulkSendBatchStatus BulkSendV2BatchGetBulkSendBatchStatus([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The batch ID.")] string bulkSendBatchId);

    [OSAction(Description = "Updates the name of a bulk send batch.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BulkSendBatchStatus BulkSendV2BatchPutBulkSendBatchStatus([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The batch ID.")] string bulkSendBatchId, [OSParameter(Description = "The JSON request body payload.")] BulkSendBatchRequest requestBody);

    [OSAction(Description = "Applies a bulk action to all envelopes from a specified bulk send.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BulkSendBatchStatus BulkSendV2BatchPutBulkSendBatchAction([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The action to apply. Valid values: * `resend` * `correct` * `void`")] string bulkAction, [OSParameter(Description = "The batch ID.")] string bulkSendBatchId, [OSParameter(Description = "The JSON request body payload.")] BulkSendBatchActionRequest requestBody);

    [OSAction(Description = "Gets envelopes from a specific bulk send batch.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopesInformation BulkSendV2EnvelopesGetBulkSendBatchEnvelopes([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The batch ID.")] string bulkSendBatchId, [OSParameter(Description = "The maximum number of results to return. Use `start_position` to specify the number of results to skip. Valid values: `1` to `1000`")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "When `recipients`, only envelopes with recipient nodes will be included in the response.")] string include = "", [OSParameter(Description = "Set to true to send the include query parameter to the API.")] bool includeInclude = false, [OSParameter(Description = "The order in which to sort the results. Valid values are: - Descending order: `desc` (default) - Ascending order: `asc`")] string order = "", [OSParameter(Description = "Set to true to send the order query parameter to the API.")] bool includeOrder = false, [OSParameter(Description = "The envelope attribute used to sort the results. Valid values are: - `created` (default) - `completed` - `last_modified` - `sent` - `status` - `subject` - `status_changed`")] string orderBy = "", [OSParameter(Description = "Set to true to send the order_by query parameter to the API.")] bool includeOrderBy = false, [OSParameter(Description = "Use this parameter to search for specific text.")] string searchText = "", [OSParameter(Description = "Set to true to send the search_text query parameter to the API.")] bool includeSearchText = false, [OSParameter(Description = "The zero-based index of the result from which to start returning results. Use with `count` to limit the number of results. The default value is `0`.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false, [OSParameter(Description = "Comma-separated list of envelope statuses. Note that `any` should not be included with other statuses. In other words, `any` is a valid parameter value, but `any,sent` is not. Use the value `deliveryfailure` to get all envelopes with `AuthFailed` and `AutoResponded` status. This value is specific to bulk sending.")] string status = "", [OSParameter(Description = "Set to true to send the status query parameter to the API.")] bool includeStatus = false);

    [OSAction(Description = "Gets bulk send lists.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BulkSendingListSummaries BulkSendV2CRUDGetBulkSendLists([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Creates a bulk send list.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BulkSendingList BulkSendV2CRUDPostBulkSendList([OSParameter(Description = "The ID of the account.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] BulkSendingList requestBody);

    [OSAction(Description = "Gets a specific bulk send list.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BulkSendingList BulkSendV2CRUDGetBulkSendList([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The GUID of the bulk send list. This property is created after you post a new bulk send list.")] string bulkSendListId);

    [OSAction(Description = "Updates a bulk send list.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BulkSendingList BulkSendV2CRUDPutBulkSendList([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The GUID of the bulk send list. This property is created after you post a new bulk send list.")] string bulkSendListId, [OSParameter(Description = "The JSON request body payload.")] BulkSendingList requestBody);

    [OSAction(Description = "Deletes a bulk send list.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BulkSendingListSummaries BulkSendV2CRUDDeleteBulkSendList([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The GUID of the bulk send list. This property is created after you post a new bulk send list.")] string bulkSendListId);

    [OSAction(Description = "Creates a bulk send request.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BulkSendResponse BulkSendV2SendPostBulkSendRequest([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The GUID of the bulk send list. This property is created after you post a new bulk send list.")] string bulkSendListId, [OSParameter(Description = "The JSON request body payload.")] BulkSendRequest requestBody);

    [OSAction(Description = "Creates a bulk send test.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BulkSendTestResponse BulkSendV2TestPostBulkSendTestRequest([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The GUID of the bulk send list. This property is created after you post a new bulk send list.")] string bulkSendListId, [OSParameter(Description = "The JSON request body payload.")] BulkSendRequest requestBody);

    [OSAction(Description = "Deletes the signature for one or more captive recipient records.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    CaptiveRecipientInformation CaptiveRecipientsDeleteCaptiveRecipientsPart([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "Signature is the only supported value.")] string recipientPart, [OSParameter(Description = "The JSON request body payload.")] CaptiveRecipientInformation requestBody);

    [OSAction(Description = "Initiate a new chunked upload.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ChunkedUploadResponse ChunkedUploadsPostChunkedUploads([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] ChunkedUploadRequest requestBody);

    [OSAction(Description = "Retrieves metadata about a chunked upload.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ChunkedUploadResponse ChunkedUploadsGetChunkedUpload([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the chunked upload.")] string chunkedUploadId, [OSParameter(Description = "(Optional) This parameter enables you to include additional attribute data in the response. The valid value for this method is `checksum`, which returns an SHA256 checksum of the content of the chunked upload in the response. You can use compare this checksum against your own checksum of the original content to verify that there are no missing parts before you attempt to commit the chunked upload.")] string include = "", [OSParameter(Description = "Set to true to send the include query parameter to the API.")] bool includeInclude = false);

    [OSAction(Description = "Commit a chunked upload.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ChunkedUploadResponse ChunkedUploadsPutChunkedUploads([OSParameter(Description = "(Required) The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "(Required) The ID of the chunked upload to commit.")] string chunkedUploadId, [OSParameter(Description = "(Required) You must use this query parameter with the value `commit`, which affirms the request to validate and prepare the chunked upload for use with other API calls.")] string action = "", [OSParameter(Description = "Set to true to send the action query parameter to the API.")] bool includeAction = false);

    [OSAction(Description = "Deletes a chunked upload.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ChunkedUploadResponse ChunkedUploadsDeleteChunkedUpload([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the chunked upload.")] string chunkedUploadId);

    [OSAction(Description = "Add a chunk to an existing chunked upload.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ChunkedUploadResponse ChunkedUploadsPutChunkedUploadPart([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the chunked upload.")] string chunkedUploadId, [OSParameter(Description = "The sequence or order of the part in the chunked upload. By default, the sequence of the first part that is uploaded as part of the Create request is `0`. **Note:** You can add parts out of order. However, the chunked upload must consist of a contiguous series of one or more parts before you can successfully commit it.")] string chunkedUploadPartSeq, [OSParameter(Description = "The JSON request body payload.")] ChunkedUploadRequest requestBody);

    [OSAction(Description = "Get Connect configuration information.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ConnectConfigResults ConnectGetConnectConfigs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Updates a specified Connect configuration.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ConnectCustomConfiguration ConnectPutConnectConfiguration([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] ConnectCustomConfiguration requestBody);

    [OSAction(Description = "Creates a Connect configuration.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ConnectCustomConfiguration ConnectPostConnectConfiguration([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] ConnectCustomConfiguration requestBody);

    [OSAction(Description = "Gets the details about a Connect configuration.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ConnectConfigResults ConnectGetConnectConfig([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the custom Connect configuration being accessed.")] string connectId);

    [OSAction(Description = "Deletes the specified Connect configuration.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] ConnectDeleteConnectConfig([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the custom Connect configuration being accessed.")] string connectId);

    [OSAction(Description = "Returns all users from the configured Connect service.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    IntegratedConnectUserInfoList ConnectGetConnectAllUsers([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the custom Connect configuration being accessed.")] string connectId, [OSParameter(Description = "The maximum number of results to return.")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "The domain_users_only query parameter.")] string domainUsersOnly = "", [OSParameter(Description = "Set to true to send the domain_users_only query parameter to the API.")] bool includeDomainUsersOnly = false, [OSParameter(Description = "Filters returned user records by full email address or a substring of email address.")] string emailSubstring = "", [OSParameter(Description = "Set to true to send the email_substring query parameter to the API.")] bool includeEmailSubstring = false, [OSParameter(Description = "The position within the total result set from which to start returning values. The value **thumbnail** may be used to return the page image.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false, [OSParameter(Description = "The status of the item.")] string status = "", [OSParameter(Description = "Set to true to send the status query parameter to the API.")] bool includeStatus = false, [OSParameter(Description = "Filters results based on a full or partial user name. **Note:** When you enter a partial user name, you do not use a wildcard character.")] string userNameSubstring = "", [OSParameter(Description = "Set to true to send the user_name_substring query parameter to the API.")] bool includeUserNameSubstring = false);

    [OSAction(Description = "Returns users from the configured Connect service.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    IntegratedUserInfoList ConnectGetConnectUsers([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the custom Connect configuration being accessed.")] string connectId, [OSParameter(Description = "The maximum number of results to return. Use `start_position` to specify the number of results to skip.")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "Filters returned user records by full email address or a substring of email address.")] string emailSubstring = "", [OSParameter(Description = "Set to true to send the email_substring query parameter to the API.")] bool includeEmailSubstring = false, [OSParameter(Description = "The list_included_users query parameter.")] string listIncludedUsers = "", [OSParameter(Description = "Set to true to send the list_included_users query parameter to the API.")] bool includeListIncludedUsers = false, [OSParameter(Description = "The zero-based index of the result from which to start returning results. Use with `count` to limit the number of results. The default value is `0`.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false, [OSParameter(Description = "Filters the results by user status. You can specify a comma-separated list of the following statuses: * ActivationRequired * ActivationSent * Active * Closed * Disabled")] string status = "", [OSParameter(Description = "Set to true to send the status query parameter to the API.")] bool includeStatus = false, [OSParameter(Description = "Filters results based on a full or partial user name. **Note:** When you enter a partial user name, you do not use a wildcard character.")] string userNameSubstring = "", [OSParameter(Description = "Set to true to send the user_name_substring query parameter to the API.")] bool includeUserNameSubstring = false);

    [OSAction(Description = "Republishes Connect information for the specified envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ConnectFailureResults ConnectPublishPutConnectRetryByEnvelope([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId);

    [OSAction(Description = "Submits a batch of historical envelopes for republish to a webhook.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopePublishTransaction HistoricalEnvelopePublishPostHistoricalEn_2a991cb6([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] ConnectHistoricalEnvelopeRepublish requestBody);

    [OSAction(Description = "Republishes Connect information for multiple envelopes.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ConnectFailureResults ConnectPublishPutConnectRetry([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] ConnectFailureFilter requestBody);

    [OSAction(Description = "Gets the Connect failure log information.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ConnectLogs ConnectFailuresGetConnectLogs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The start date for a date range in UTC DateTime format. **Note:** If this property is null, no date filtering is applied.")] string fromDate = "", [OSParameter(Description = "Set to true to send the from_date query parameter to the API.")] bool includeFromDate = false, [OSParameter(Description = "The end of a search date range in UTC DateTime format. When you use this parameter, only templates created up to this date and time are returned. **Note:** If this property is null, the value defaults to the current date.")] string toDate = "", [OSParameter(Description = "Set to true to send the to_date query parameter to the API.")] bool includeToDate = false);

    [OSAction(Description = "Deletes a Connect failure log entry.", ReturnDescription = "The result returned by the API.", ReturnName = "Json")]
    string ConnectFailuresDeleteConnectFailureLog([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the Connect post failure. Use `all` to delete all failures for the account.")] string failureId);

    [OSAction(Description = "Gets the Connect log.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ConnectLogs ConnectLogGetConnectLogs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The start date for a date range in UTC DateTime format. **Note:** If this property is null, no date filtering is applied.")] string fromDate = "", [OSParameter(Description = "Set to true to send the from_date query parameter to the API.")] bool includeFromDate = false, [OSParameter(Description = "The end of a search date range in UTC DateTime format. When you use this parameter, only templates created up to this date and time are returned. **Note:** If this property is null, the value defaults to the current date.")] string toDate = "", [OSParameter(Description = "Set to true to send the to_date query parameter to the API.")] bool includeToDate = false);

    [OSAction(Description = "Deletes a list of Connect log entries.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] ConnectLogDeleteConnectLogs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Gets a Connect log entry.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ConnectLog ConnectLogGetConnectLog([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the Connect log entry.")] string logId, [OSParameter(Description = "When **true,** the response includes the `connectDebugLog` information.")] string additionalInfo = "", [OSParameter(Description = "Set to true to send the additional_info query parameter to the API.")] bool includeAdditionalInfo = false);

    [OSAction(Description = "Deletes a specified Connect log entry.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] ConnectLogDeleteConnectLog([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the Connect log entry. Use `all` to delete all entries for the account.")] string logId);

    [OSAction(Description = "Retrieves the Connect OAuth information for the account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ConnectOAuthConfig ConnectOAuthConfigGetConnectOAuthConfig([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Updates the existing Connect OAuth configuration for the account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ConnectOAuthConfig ConnectOAuthConfigPutConnectOAuthConfig([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] ConnectOAuthConfig requestBody);

    [OSAction(Description = "Set up Connect OAuth for the specified account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ConnectOAuthConfig ConnectOAuthConfigPostConnectOAuthConfig([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] ConnectOAuthConfig requestBody);

    [OSAction(Description = "Delete the Connect OAuth configuration.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] ConnectOAuthConfigDeleteConnectOAuthConfig([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Gets the default Electronic Record and Signature Disclosure for an account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountConsumerDisclosures ConsumerDisclosureGetConsumerDisclosure([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The code for the signer language version of the disclosure that you want to retrieve. The following languages are supported: - Arabic (`ar`) - Bulgarian (`bg`) - Czech (`cs`) - Chinese Simplified (`zh_CN`) - Chinese Traditional (`zh_TW`) - Croatian (`hr`) - Danish (`da`) - Dutch (`nl`) - English US (`en`) - English UK (`en_GB`) - Estonian (`et`) - Farsi (`fa`) - Finnish (`fi`) - French (`fr`) - French Canadian (`fr_CA`) - German (`de`) - Greek (`el`) - Hebrew (`he`) - Hindi (`hi`) - Hungarian (`hu`) - Bahasa Indonesian (`id`) - Italian (`it`) - Japanese (`ja`) - Korean (`ko`) - Latvian (`lv`) - Lithuanian (`lt`) - Bahasa Melayu (`ms`) - Norwegian (`no`) - Polish (`pl`) - Portuguese (`pt`) - Portuguese Brazil (`pt_BR`) - Romanian (`ro`) - Russian (`ru`) - Serbian (`sr`) - Slovak (`sk`) - Slovenian (`sl`) - Spanish (`es`) - Spanish Latin America (`es_MX`) - Swedish (`sv`) - Thai (`th`) - Turkish (`tr`) - Ukrainian (`uk`) - Vietnamese (`vi`) Additionally, you can automatically detect the browser language being used by the viewer and display the disclosure in that language by setting the value to `browser`.")] string langCode = "", [OSParameter(Description = "Set to true to send the langCode query parameter to the API.")] bool includeLangCode = false);

    [OSAction(Description = "Gets the Electronic Record and Signature Disclosure for an account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountConsumerDisclosures ConsumerDisclosureGetConsumerDisclosureLangCode([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The code for the signer language version of the disclosure that you want to retrieve. The following languages are supported: - Arabic (`ar`) - Bulgarian (`bg`) - Czech (`cs`) - Chinese Simplified (`zh_CN`) - Chinese Traditional (`zh_TW`) - Croatian (`hr`) - Danish (`da`) - Dutch (`nl`) - English US (`en`) - English UK (`en_GB`) - Estonian (`et`) - Farsi (`fa`) - Finnish (`fi`) - French (`fr`) - French Canadian (`fr_CA`) - German (`de`) - Greek (`el`) - Hebrew (`he`) - Hindi (`hi`) - Hungarian (`hu`) - Bahasa Indonesian (`id`) - Italian (`it`) - Japanese (`ja`) - Korean (`ko`) - Latvian (`lv`) - Lithuanian (`lt`) - Bahasa Melayu (`ms`) - Norwegian (`no`) - Polish (`pl`) - Portuguese (`pt`) - Portuguese Brazil (`pt_BR`) - Romanian (`ro`) - Russian (`ru`) - Serbian (`sr`) - Slovak (`sk`) - Slovenian (`sl`) - Spanish (`es`) - Spanish Latin America (`es_MX`) - Swedish (`sv`) - Thai (`th`) - Turkish (`tr`) - Ukrainian (`uk`) - Vietnamese (`vi`) Additionally, you can automatically detect the browser language being used by the viewer and display the disclosure in that language by setting the value to `browser`.")] string langCode);

    [OSAction(Description = "Updates the Electronic Record and Signature Disclosure for an account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ConsumerDisclosure ConsumerDisclosurePutConsumerDisclosure([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The code for the signer language version of the disclosure that you want to update. The following languages are supported: - Arabic (`ar`) - Bulgarian (`bg`) - Czech (`cs`) - Chinese Simplified (`zh_CN`) - Chinese Traditional (`zh_TW`) - Croatian (`hr`) - Danish (`da`) - Dutch (`nl`) - English US (`en`) - English UK (`en_GB`) - Estonian (`et`) - Farsi (`fa`) - Finnish (`fi`) - French (`fr`) - French Canadian (`fr_CA`) - German (`de`) - Greek (`el`) - Hebrew (`he`) - Hindi (`hi`) - Hungarian (`hu`) - Bahasa Indonesian (`id`) - Italian (`it`) - Japanese (`ja`) - Korean (`ko`) - Latvian (`lv`) - Lithuanian (`lt`) - Bahasa Melayu (`ms`) - Norwegian (`no`) - Polish (`pl`) - Portuguese (`pt`) - Portuguese Brazil (`pt_BR`) - Romanian (`ro`) - Russian (`ru`) - Serbian (`sr`) - Slovak (`sk`) - Slovenian (`sl`) - Spanish (`es`) - Spanish Latin America (`es_MX`) - Swedish (`sv`) - Thai (`th`) - Turkish (`tr`) - Ukrainian (`uk`) - Vietnamese (`vi`) Additionally, you can automatically detect the browser language being used by the viewer and display the disclosure in that language by setting the value to `browser`.")] string langCode, [OSParameter(Description = "The JSON request body payload.")] ConsumerDisclosure requestBody, [OSParameter(Description = "(Optional) When true, the response includes metadata indicating which properties are editable.")] string includeMetadata = "", [OSParameter(Description = "Set to true to send the include_metadata query parameter to the API.")] bool includeIncludeMetadata = false);

    [OSAction(Description = "Updates one or more contacts.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ContactUpdateResponse ContactsPutContacts([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] ContactModRequest requestBody);

    [OSAction(Description = "Add contacts to a contacts list.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ContactUpdateResponse ContactsPostContacts([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] ContactModRequest requestBody);

    [OSAction(Description = "Deletes multiple contacts from an account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ContactUpdateResponse ContactsDeleteContacts([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] ContactModRequest requestBody);

    [OSAction(Description = "Gets one or more contacts.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ContactGetResponse ContactsGetContactById([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of a contact person in the account's address book. **Note:** To return all contacts, omit this parameter. It is not required.")] string contactId, [OSParameter(Description = "(Optional) The cloud provider from which to retrieve the contacts. Valid values are: - `rooms` - `docusignCore` (default)")] string cloudProvider = "", [OSParameter(Description = "Set to true to send the cloud_provider query parameter to the API.")] bool includeCloudProvider = false);

    [OSAction(Description = "Deletes a contact.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ContactUpdateResponse ContactsDeleteContactWithId([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of a contact person in the account's address book.")] string contactId);

    [OSAction(Description = "Gets a list of custom fields.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountCustomFields AccountCustomFieldsGetAccountCustomFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Creates an account custom field.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountCustomFields AccountCustomFieldsPostAccountCustomFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] CustomField requestBody, [OSParameter(Description = "(Optional) When **true,** the new custom field is applied to all of the templates on the account.")] string applyToTemplates = "", [OSParameter(Description = "Set to true to send the apply_to_templates query parameter to the API.")] bool includeApplyToTemplates = false);

    [OSAction(Description = "Updates an account custom field.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountCustomFields AccountCustomFieldsPutAccountCustomFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the custom field.")] string customFieldId, [OSParameter(Description = "The JSON request body payload.")] CustomField requestBody, [OSParameter(Description = "The apply_to_templates query parameter.")] string applyToTemplates = "", [OSParameter(Description = "Set to true to send the apply_to_templates query parameter to the API.")] bool includeApplyToTemplates = false);

    [OSAction(Description = "Deletes an account custom field.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] AccountCustomFieldsDeleteAccountCustomFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the custom field.")] string customFieldId, [OSParameter(Description = "The apply_to_templates query parameter.")] string applyToTemplates = "", [OSParameter(Description = "Set to true to send the apply_to_templates query parameter to the API.")] bool includeApplyToTemplates = false);

    [OSAction(Description = "Search for specific sets of envelopes by using search filters.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopesInformation EnvelopesGetEnvelopes([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "Specifies the authoritative copy status for the envelopes. Valid values: * `Unknown` * `Original` * `Transferred` * `AuthoritativeCopy` * `AuthoritativeCopyExportPending` * `AuthoritativeCopyExported` * `DepositPending` * `Deposited` * `DepositedEO` * `DepositFailed`")] string acStatus = "", [OSParameter(Description = "Set to true to send the ac_status query parameter to the API.")] bool includeAcStatus = false, [OSParameter(Description = "Reserved for Docusign.")] string block = "", [OSParameter(Description = "Set to true to send the block query parameter to the API.")] bool includeBlock = false, [OSParameter(Description = "Reserved for Docusign.")] string cdseMode = "", [OSParameter(Description = "Set to true to send the cdse_mode query parameter to the API.")] bool includeCdseMode = false, [OSParameter(Description = "Reserved for Docusign.")] string continuationToken = "", [OSParameter(Description = "Set to true to send the continuation_token query parameter to the API.")] bool includeContinuationToken = false, [OSParameter(Description = "The maximum number of results to return. The maximum value is 1000. To get the next or previous set of envelopes, use `nextUri` or `previousUri` from the response.")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "Optional. Specifies an envelope custom field name and value searched for in the envelopes. Format: `custom_envelope_field_name=desired_value` Example: If you have an envelope custom field named \"Region\" and you want to search for all envelopes where the value is \"West\" you would use set this parameter to `Region=West`.")] string customField = "", [OSParameter(Description = "Set to true to send the custom_field query parameter to the API.")] bool includeCustomField = false, [OSParameter(Description = "Limit results to envelopes sent by the account user with this email address. `user_name` must be given as well, and both `email` and `user_name` must refer to an existing account user.")] string email = "", [OSParameter(Description = "Set to true to send the email query parameter to the API.")] bool includeEmail = false, [OSParameter(Description = "Comma separated list of `envelopeId` values.")] string envelopeIds = "", [OSParameter(Description = "Set to true to send the envelope_ids query parameter to the API.")] bool includeEnvelopeIds = false, [OSParameter(Description = "Excludes information from the response. Enter as a comma-separated list (e.g., `folders,powerforms`). Valid values: - `recipients` - `powerforms` - `folders`")] string exclude = "", [OSParameter(Description = "Set to true to send the exclude query parameter to the API.")] bool includeExclude = false, [OSParameter(Description = "Returns the envelopes from specific folders. Enter as a comma-separated list of either valid folder GUIDs or the following values: - `awaiting_my_signature` - `completed` - `draft` - `drafts` - `expiring_soon` - `inbox` - `out_for_signature` - `recyclebin` - `sentitems` - `waiting_for_others`")] string folderIds = "", [OSParameter(Description = "Set to true to send the folder_ids query parameter to the API.")] bool includeFolderIds = false, [OSParameter(Description = "Returns the envelopes from folders of a specific type. Enter as a comma-separated list of the following values: - `normal` - `inbox` - `sentitems` - `draft` - `templates`")] string folderTypes = "", [OSParameter(Description = "Set to true to send the folder_types query parameter to the API.")] bool includeFolderTypes = false, [OSParameter(Description = "Specifies the date and time to start looking for status changes. This parameter is required unless `envelopeIds` or `transactionIds` are set. Although you can use any date format supported by the .NET system library's [`DateTime.Parse()`][msoft] function, Docusign recommends using [ISO 8601][] format dates with an explicit time zone offset. If you do not provide a time zone offset, the method uses the server's time zone. For example, the following dates and times refer to the same instant: * `2017-05-02T01:44Z` * `2017-05-01T21:44-04:00` * `2017-05-01T18:44-07:00` If this property is not included, envelopes from the last two years will be returned. [msoft]: https://docs.microsoft.com/en-us/dotnet/api/system.datetime.parse?redirectedfrom=MSDN&view=net-5.0#overloads [ISO 8601]: https://en.wikipedia.org/wiki/ISO_8601")] string fromDate = "", [OSParameter(Description = "Set to true to send the from_date query parameter to the API.")] bool includeFromDate = false, [OSParameter(Description = "This is the status type checked for in the `from_date`/`to_date` period. For example, if `Created` is specified, then envelopes created during the period are found. If `Changed` is specified, then envelopes that changed status during the period are returned. The default value is `Changed`. Valid values: * `Changed` * `Voided` * `Created` * `Deleted` * `Sent` * `Delivered` * `Signed` * `Completed` * `Declined` * `TimedOut` * `Processing`")] string fromToStatus = "", [OSParameter(Description = "Set to true to send the from_to_status query parameter to the API.")] bool includeFromToStatus = false, [OSParameter(Description = "Specifies additional information to return  about the envelopes. Use a comma-separated list, such as `folders, recipients` to specify information. Valid values are: - `custom_fields`: The custom fields associated with the envelope. - `documents`: The documents associated with the envelope. - `attachments`: The attachments associated with the envelope. - `extensions`: Information about the email settings associated with the envelope. - `folders`: The folders where the envelope exists. - `recipients`: The recipients associated with the envelope. - `payment_tabs`: The payment tabs associated with the envelope.")] string include = "", [OSParameter(Description = "Set to true to send the include query parameter to the API.")] bool includeInclude = false, [OSParameter(Description = "When **true,** information about envelopes that have been deleted is included in the response.")] string includePurgeInformation = "", [OSParameter(Description = "Set to true to send the include_purge_information query parameter to the API.")] bool includeIncludePurgeInformation = false, [OSParameter(Description = "A comma-separated list of folders from which you want to get envelopes. Valid values: - `normal` - `inbox` - `sentitems` - `draft` - `templates`")] string intersectingFolderIds = "", [OSParameter(Description = "Set to true to send the intersecting_folder_ids query parameter to the API.")] bool includeIntersectingFolderIds = false, [OSParameter(Description = "Returns envelopes that were modified prior to the specified date and time. Example: `2020-05-09T21:56:12.2500000Z`")] string lastQueriedDate = "", [OSParameter(Description = "Set to true to send the last_queried_date query parameter to the API.")] bool includeLastQueriedDate = false, [OSParameter(Description = "Returns envelopes in either ascending (`asc`) or descending (`desc`) order.")] string order = "", [OSParameter(Description = "Set to true to send the order query parameter to the API.")] bool includeOrder = false, [OSParameter(Description = "Sorts results according to a specific property. Valid values: - `last_modified` - `action_required` - `created` - `completed` - `envelope_name` - `expire` - `sent` - `signer_list` - `status` - `subject` - `user_name` - `status_changed` - `last_modified`")] string orderBy = "", [OSParameter(Description = "Set to true to send the order_by query parameter to the API.")] bool includeOrderBy = false, [OSParameter(Description = "A comma-separated list of `PowerFormId` values.")] string powerformids = "", [OSParameter(Description = "Set to true to send the powerformids query parameter to the API.")] bool includePowerformids = false, [OSParameter(Description = "The time in seconds that the query should run before returning data.")] string queryBudget = "", [OSParameter(Description = "Set to true to send the query_budget query parameter to the API.")] bool includeQueryBudget = false, [OSParameter(Description = "The requester_date_format query parameter.")] string requesterDateFormat = "", [OSParameter(Description = "Set to true to send the requester_date_format query parameter to the API.")] bool includeRequesterDateFormat = false, [OSParameter(Description = "The search_mode query parameter.")] string searchMode = "", [OSParameter(Description = "Set to true to send the search_mode query parameter to the API.")] bool includeSearchMode = false, [OSParameter(Description = "Free text search criteria that you can use to filter the list of envelopes that is returned.")] string searchText = "", [OSParameter(Description = "Set to true to send the search_text query parameter to the API.")] bool includeSearchText = false, [OSParameter(Description = "The zero-based index of the result from which to start returning results. Use with `count` to limit the number of results. The default value is `0`.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false, [OSParameter(Description = "A comma-separated list of current envelope statuses to be included in the response. Valid values: * `completed` * `created` * `declined` * `deleted` * `delivered` * `processing` * `sent` * `signed` * `timedout` * `voided` The `any` value is equivalent to any status.")] string status = "", [OSParameter(Description = "Set to true to send the status query parameter to the API.")] bool includeStatus = false, [OSParameter(Description = "Specifies the date and time to stop looking for status changes. The default is the current date and time. Although you can use any date format supported by the .NET system library's [`DateTime.Parse()`][msoft] function, Docusign recommends using [ISO 8601][] format dates with an explicit time zone offset If you do not provide a time zone offset, the method uses the server's time zone. For example, the following dates and times refer to the same instant: * `2017-05-02T01:44Z` * `2017-05-01T21:44-04:00` * `2017-05-01T18:44-07:00` [msoft]: https://docs.microsoft.com/en-us/dotnet/api/system.datetime.parse?redirectedfrom=MSDN&view=net-5.0#overloads [ISO 8601]: https://en.wikipedia.org/wiki/ISO_8601")] string toDate = "", [OSParameter(Description = "Set to true to send the to_date query parameter to the API.")] bool includeToDate = false, [OSParameter(Description = "A comma-separated list of envelope transaction IDs. Getting envelope status by transaction IDs is useful for offline signing situations to determine if an envelope was created or not. It can be used for the cases where a network connection was lost before the envelope status could be returned. **Note:** Transaction IDs are only valid in the Docusign system for seven days.")] string transactionIds = "", [OSParameter(Description = "Set to true to send the transaction_ids query parameter to the API.")] bool includeTransactionIds = false, [OSParameter(Description = "Returns envelopes where the current user is the recipient, the sender, or the recipient only. (For example, `user_filter=sender`.) Valid values are: - `sender` - `recipient` - `recipient_only`")] string userFilter = "", [OSParameter(Description = "Set to true to send the user_filter query parameter to the API.")] bool includeUserFilter = false, [OSParameter(Description = "The ID of the user who created the envelopes to be retrieved. Note that an account can have multiple users, and any user with account access can retrieve envelopes by user_id from the account.")] string userId = "", [OSParameter(Description = "Set to true to send the user_id query parameter to the API.")] bool includeUserId = false, [OSParameter(Description = "Limit results to envelopes sent by the account user with this user name. `email` must be given as well, and both `email` and `user_name` must refer to an existing account user.")] string userName = "", [OSParameter(Description = "Set to true to send the user_name query parameter to the API.")] bool includeUserName = false);

    [OSAction(Description = "Creates an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeSummary EnvelopesPostEnvelopes([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeDefinition requestBody, [OSParameter(Description = "Reserved for Docusign.")] string cdseMode = "", [OSParameter(Description = "Set to true to send the cdse_mode query parameter to the API.")] bool includeCdseMode = false, [OSParameter(Description = "When true, users can define the routing order of recipients while sending documents for signature.")] string changeRoutingOrder = "", [OSParameter(Description = "Set to true to send the change_routing_order query parameter to the API.")] bool includeChangeRoutingOrder = false, [OSParameter(Description = "Reserved for Docusign.")] string completedDocumentsOnly = "", [OSParameter(Description = "Set to true to send the completed_documents_only query parameter to the API.")] bool includeCompletedDocumentsOnly = false, [OSParameter(Description = "When **true,** template roles will be merged, and empty recipients will be removed. This parameter applies when you create a draft envelope with multiple templates. (To create a draft envelope, the `status` field is set to `created`.) **Note:** Docusign recommends that this parameter should be set to **true** whenever you create a draft envelope with multiple templates.")] string mergeRolesOnDraft = "", [OSParameter(Description = "Set to true to send the merge_roles_on_draft query parameter to the API.")] bool includeMergeRolesOnDraft = false);

    [OSAction(Description = "Gets the status of a single envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Envelope EnvelopesGetEnvelope([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "When **true,** envelope information can be added or modified.")] string advancedUpdate = "", [OSParameter(Description = "Set to true to send the advanced_update query parameter to the API.")] bool includeAdvancedUpdate = false, [OSParameter(Description = "Specifies additional information about the envelope to return. Enter a comma-separated list, such as `tabs,recipients`. Valid values are: - `custom_fields`: The custom fields associated with the envelope. - `documents`: The documents associated with the envelope. - `attachments`: The attachments associated with the envelope. - `extensions`: The email settings associated with the envelope. - `folders`: The folder where the envelope exists. - `recipients`: The recipients associated with the envelope. - `powerform`: The PowerForms associated with the envelope. - `prefill_tabs`: The pre-filled tabs associated with the envelope. - `tabs`: The tabs associated with the envelope. - `payment_tabs`: The payment tabs associated with the envelope. - `workflow`: The workflow definition associated with the envelope.")] string include = "", [OSParameter(Description = "Set to true to send the include query parameter to the API.")] bool includeInclude = false, [OSParameter(Description = "When **true,** all tabs with anchor tab properties are included in the response. The default value is **false.**")] string includeAnchorTabLocations = "", [OSParameter(Description = "Set to true to send the include_anchor_tab_locations query parameter to the API.")] bool includeIncludeAnchorTabLocations = false, [OSParameter(Description = "The user_id query parameter.")] string userId = "", [OSParameter(Description = "Set to true to send the user_id query parameter to the API.")] bool includeUserId = false);

    [OSAction(Description = "Send, void, or modify a draft envelope. Purge documents from a completed envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeUpdateSummary EnvelopesPutEnvelope([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] Envelope requestBody, [OSParameter(Description = "When **true,** allows the caller to update recipients, tabs, custom fields, notification, email settings and other envelope attributes.")] string advancedUpdate = "", [OSParameter(Description = "Set to true to send the advanced_update query parameter to the API.")] bool includeAdvancedUpdate = false, [OSParameter(Description = "The recycle_on_void query parameter.")] string recycleOnVoid = "", [OSParameter(Description = "Set to true to send the recycle_on_void query parameter to the API.")] bool includeRecycleOnVoid = false, [OSParameter(Description = "When **true,** sends the specified envelope again.")] string resendEnvelope = "", [OSParameter(Description = "Set to true to send the resend_envelope query parameter to the API.")] bool includeResendEnvelope = false);

    [OSAction(Description = "Returns a list of envelope attachments associated with a specified envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeAttachmentsResult AttachmentsGetAttachments([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId);

    [OSAction(Description = "Adds one or more envelope attachments to a draft or in-process envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeAttachmentsResult AttachmentsPutAttachments([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeAttachmentsRequest requestBody);

    [OSAction(Description = "Deletes one or more envelope attachments from a draft envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeAttachmentsResult AttachmentsDeleteAttachments([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeAttachmentsRequest requestBody);

    [OSAction(Description = "Retrieves an envelope attachment from an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] AttachmentsGetAttachment([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique identifier for the attachment.")] string attachmentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId);

    [OSAction(Description = "Updates an envelope attachment in a draft or in-process envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeAttachmentsResult AttachmentsPutAttachment([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique identifier for the attachment.")] string attachmentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] Attachment requestBody);

    [OSAction(Description = "Gets the envelope audit events for an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeAuditEventResponse AuditEventsGetAuditEvents([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The user's locale code. Valid values are: - `zh_CN` - `zh_TW` - `nl` - `en` - `fr` - `de` - `it` - `ja` - `ko` - `pt` - `pt_BR` - `ru` - `es`")] string locale = "", [OSParameter(Description = "Set to true to send the locale query parameter to the API.")] bool includeLocale = false);

    [OSAction(Description = "Gets a PDF transcript of all of the comments in an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] CommentsGetCommentsTranscript([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "(Optional) The encoding to use for the file.")] string encoding = "", [OSParameter(Description = "Set to true to send the encoding query parameter to the API.")] bool includeEncoding = false);

    [OSAction(Description = "Gets the custom field information for the specified envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    CustomFieldsEnvelope CustomFieldsGetCustomFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId);

    [OSAction(Description = "Updates envelope custom fields in an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeCustomFields CustomFieldsPutCustomFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeCustomFields requestBody);

    [OSAction(Description = "Creates envelope custom fields for an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeCustomFields CustomFieldsPostCustomFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeCustomFields requestBody);

    [OSAction(Description = "Deletes envelope custom fields for draft and in-process envelopes.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeCustomFields CustomFieldsDeleteCustomFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeCustomFields requestBody);

    [OSAction(Description = "Returns sender fields for an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DocGenFormFieldResponse DocGenFormFieldsGetEnvelopeDocGenFormFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId);

    [OSAction(Description = "Updates sender fields for an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DocGenFormFieldResponse DocGenFormFieldsPutEnvelopeDocGenFormFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] DocGenFormFieldRequest requestBody, [OSParameter(Description = "When **false** or omitted, the documents are updated. When **true,** only the form fields are updated. The documents are unchanged.")] string updateDocgenFormfieldsOnly = "", [OSParameter(Description = "Set to true to send the update_docgen_formfields_only query parameter to the API.")] bool includeUpdateDocgenFormfieldsOnly = false);

    [OSAction(Description = "Gets a list of documents in an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeDocumentsResult DocumentsGetDocuments([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "When **true,** allows recipients to get documents by their user id. For example, if a user is included in two different routing orders with different visibilities, using this parameter returns all of the documents from both routing orders.")] string documentsByUserid = "", [OSParameter(Description = "Set to true to send the documents_by_userid query parameter to the API.")] bool includeDocumentsByUserid = false, [OSParameter(Description = "The include_agreement_type query parameter.")] string includeAgreementType = "", [OSParameter(Description = "Set to true to send the include_agreement_type query parameter to the API.")] bool includeIncludeAgreementType = false, [OSParameter(Description = "Reserved for Docusign.")] string includeDocgenFormfields = "", [OSParameter(Description = "Set to true to send the include_docgen_formfields query parameter to the API.")] bool includeIncludeDocgenFormfields = false, [OSParameter(Description = "When **true,** the response includes metadata that indicates which properties the sender can edit.")] string includeMetadata = "", [OSParameter(Description = "Set to true to send the include_metadata query parameter to the API.")] bool includeIncludeMetadata = false, [OSParameter(Description = "Reserved for Docusign.")] string includeTabs = "", [OSParameter(Description = "Set to true to send the include_tabs query parameter to the API.")] bool includeIncludeTabs = false, [OSParameter(Description = "Allows the sender to retrieve the documents as one of the recipients that they control. The `documents_by_userid` parameter must be set to **false** for this to work.")] string recipientId = "", [OSParameter(Description = "Set to true to send the recipient_id query parameter to the API.")] bool includeRecipientId = false, [OSParameter(Description = "The ID of a shared user that you want to impersonate in order to retrieve their view of the list of documents. This parameter is used in the context of a shared inbox (i.e., when you share envelopes from one user to another through the Docusign Admin console).")] string sharedUserId = "", [OSParameter(Description = "Set to true to send the shared_user_id query parameter to the API.")] bool includeSharedUserId = false);

    [OSAction(Description = "Adds one or more documents to an existing envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeDocumentsResult DocumentsPutDocuments([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeDefinition requestBody);

    [OSAction(Description = "Deletes documents from a draft envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeDocumentsResult DocumentsDeleteDocuments([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeDefinition requestBody);

    [OSAction(Description = "Retrieves a single document or all documents from an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] DocumentsGetDocument([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the document to retrieve. Alternatively, you can use one of the following special keywords: - `combined`: Retrieves all of the documents as a single PDF file. When the query parameter `certificate` is **true,** the certificate of completion is included in the PDF file. When the query parameter `certificate` is **false,** the certificate of completion is not included in the PDF file. - `archive`: Retrieves a ZIP archive that contains all of the PDF documents and the certificate of completion. - `certificate`: Retrieves only the certificate of completion as a PDF file. - `portfolio`: Retrieves the envelope documents as a [PDF portfolio](https://helpx.adobe.com/acrobat/using/overview-pdf-portfolios.html).")] string documentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "Used only when the `documentId` parameter is the special keyword `combined`. When **true,** the certificate of completion is included in the combined PDF file. When **false,** (the default) the certificate of completion is not included in the combined PDF file.")] string certificate = "", [OSParameter(Description = "Set to true to send the certificate query parameter to the API.")] bool includeCertificate = false, [OSParameter(Description = "When **true,** allows recipients to get documents by their user id. For example, if a user is included in two different routing orders with different visibilities, using this parameter returns all of the documents from both routing orders.")] string documentsByUserid = "", [OSParameter(Description = "Set to true to send the documents_by_userid query parameter to the API.")] bool includeDocumentsByUserid = false, [OSParameter(Description = "Reserved for Docusign.")] string encoding = "", [OSParameter(Description = "Set to true to send the encoding query parameter to the API.")] bool includeEncoding = false, [OSParameter(Description = "When **true,** the PDF bytes returned in the response are encrypted for all the key managers configured on your Docusign account. You can decrypt the documents by using the Key Manager DecryptDocument API method. For more information about Key Manager, see the Docusign Security Appliance Installation Guide that your organization received from Docusign.")] string encrypt = "", [OSParameter(Description = "Set to true to send the encrypt query parameter to the API.")] bool includeEncrypt = false, [OSParameter(Description = "Specifies the language for the Certificate of Completion in the response. The supported languages are: Chinese Simplified (zh_CN), Chinese Traditional (zh_TW), Dutch (nl), English US (en), French (fr), German (de), Italian (it), Japanese (ja), Korean (ko), Portuguese (pt), Portuguese (Brazil) (pt_BR), Russian (ru), Spanish (es).")] string language = "", [OSParameter(Description = "Set to true to send the language query parameter to the API.")] bool includeLanguage = false, [OSParameter(Description = "Allows the sender to retrieve the documents as one of the recipients that they control. The `documents_by_userid` parameter must be set to **false** for this functionality to work.")] string recipientId = "", [OSParameter(Description = "Set to true to send the recipient_id query parameter to the API.")] bool includeRecipientId = false, [OSParameter(Description = "The ID of a shared user that you want to impersonate in order to retrieve their view of the list of documents. This parameter is used in the context of a shared inbox (i.e., when you share envelopes from one user to another through the Docusign Admin console).")] string sharedUserId = "", [OSParameter(Description = "Set to true to send the shared_user_id query parameter to the API.")] bool includeSharedUserId = false, [OSParameter(Description = "When **true,** any changed fields for the returned PDF are highlighted in yellow and optional signatures or initials outlined in red. The account must have the **Highlight Data Changes** feature enabled.")] string showChanges = "", [OSParameter(Description = "Set to true to send the show_changes query parameter to the API.")] bool includeShowChanges = false, [OSParameter(Description = "When **true,** the account has the watermark feature enabled, and the envelope is not complete, then the watermark for the account is added to the PDF documents. This option can remove the watermark.")] string watermark = "", [OSParameter(Description = "Set to true to send the watermark query parameter to the API.")] bool includeWatermark = false);

    [OSAction(Description = "Adds or replaces a document in an existing envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeDocument DocumentsPutDocument([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The binary request body payload.")] byte[] requestBody);

    [OSAction(Description = "Gets the custom document fields from an  existing envelope document.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeDocumentFields DocumentFieldsGetDocumentFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId);

    [OSAction(Description = "Updates existing custom document fields in an existing envelope document.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeDocumentFields DocumentFieldsPutDocumentFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeDocumentFields requestBody);

    [OSAction(Description = "Creates custom document fields in an existing envelope document.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeDocumentFields DocumentFieldsPostDocumentFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeDocumentFields requestBody);

    [OSAction(Description = "Deletes custom document fields from an existing envelope document.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeDocumentFields DocumentFieldsDeleteDocumentFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeDocumentFields requestBody);

    [OSAction(Description = "Retrieves the HTML definition used to generate a dynamically sized responsive document.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DocumentHtmlDefinitionOriginals ResponsiveHtmlGetEnvelopeDocumentHtmlDefinitions([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The GUID of the document. Example: c671747c-xxxx-xxxx-xxxx-4a4a48e23744")] string documentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId);

    [OSAction(Description = "Returns document page images based on input.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    PageImages PagesGetPageImages([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The maximum number of results to return.")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "The number of dots per inch (DPI) for the resulting images. Valid values are 1-310 DPI. The default value is 94.")] string dpi = "", [OSParameter(Description = "Set to true to send the dpi query parameter to the API.")] bool includeDpi = false, [OSParameter(Description = "Sets the maximum height of the returned images in pixels.")] string maxHeight = "", [OSParameter(Description = "Set to true to send the max_height query parameter to the API.")] bool includeMaxHeight = false, [OSParameter(Description = "Sets the maximum width of the returned images in pixels.")] string maxWidth = "", [OSParameter(Description = "Set to true to send the max_width query parameter to the API.")] bool includeMaxWidth = false, [OSParameter(Description = "When **true,** using cache is disabled and image information is retrieved from a database. **True** is the default value.")] string nocache = "", [OSParameter(Description = "Set to true to send the nocache query parameter to the API.")] bool includeNocache = false, [OSParameter(Description = "When **true,** changes display in the user interface.")] string showChanges = "", [OSParameter(Description = "Set to true to send the show_changes query parameter to the API.")] bool includeShowChanges = false, [OSParameter(Description = "The position within the total result set from which to start returning values. The value **thumbnail** may be used to return the page image.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false);

    [OSAction(Description = "Deletes a page from a document in an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] PagesDeletePage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The page number being accessed.")] string pageNumber);

    [OSAction(Description = "Gets a page image from an envelope for display.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] PagesGetPageImage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The page number being accessed.")] string pageNumber, [OSParameter(Description = "Sets the dots per inch (DPI) for the returned image.")] string dpi = "", [OSParameter(Description = "Set to true to send the dpi query parameter to the API.")] bool includeDpi = false, [OSParameter(Description = "Sets the maximum height for the page image in pixels. The DPI is recalculated based on this setting.")] string maxHeight = "", [OSParameter(Description = "Set to true to send the max_height query parameter to the API.")] bool includeMaxHeight = false, [OSParameter(Description = "Sets the maximum width for the page image in pixels. The DPI is recalculated based on this setting.")] string maxWidth = "", [OSParameter(Description = "Set to true to send the max_width query parameter to the API.")] bool includeMaxWidth = false, [OSParameter(Description = "When **true,** changes display in the user interface.")] string showChanges = "", [OSParameter(Description = "Set to true to send the show_changes query parameter to the API.")] bool includeShowChanges = false);

    [OSAction(Description = "Rotates page image from an envelope for display.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] PagesPutPageImage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The page number being accessed.")] string pageNumber, [OSParameter(Description = "The JSON request body payload.")] PageRequest requestBody);

    [OSAction(Description = "Returns tabs on the specified page.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeDocumentTabs TabsGetPageTabs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The page number being accessed.")] string pageNumber);

    [OSAction(Description = "Creates a preview of the responsive version of a document.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DocumentHtmlDefinitions ResponsiveHtmlPostDocumentResponsiveHtmlPreview([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] DocumentHtmlDefinition requestBody);

    [OSAction(Description = "Returns the tabs on a document.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeDocumentTabs TabsGetDocumentTabs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "When **true,** the response includes metadata indicating which properties are editable.")] string includeMetadata = "", [OSParameter(Description = "Set to true to send the include_metadata query parameter to the API.")] bool includeIncludeMetadata = false, [OSParameter(Description = "Filters for tabs that occur on the pages that you specify. Enter as a comma-separated list of page GUIDs. Example: `page_numbers=2,6` Note: You can only enter individual page numbers, and not a page range.")] string pageNumbers = "", [OSParameter(Description = "Set to true to send the page_numbers query parameter to the API.")] bool includePageNumbers = false);

    [OSAction(Description = "Updates the tabs for document.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Tabs TabsPutDocumentTabs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] Tabs requestBody);

    [OSAction(Description = "Adds tabs to a document in an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Tabs TabsPostDocumentTabs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] Tabs requestBody);

    [OSAction(Description = "Deletes tabs from a document in an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Tabs TabsDeleteDocumentTabs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] Tabs requestBody);

    [OSAction(Description = "Gets the templates associated with a document in an existing envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    TemplateInformation TemplatesGetDocumentTemplates([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "A comma-separated list that limits the results. Valid values are: * `applied` * `matched`")] string include = "", [OSParameter(Description = "Set to true to send the include query parameter to the API.")] bool includeInclude = false);

    [OSAction(Description = "Adds templates to a document in an  envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DocumentTemplateList TemplatesPostDocumentTemplates([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] DocumentTemplateList requestBody, [OSParameter(Description = "If omitted or set to false (the default), envelope recipients _will be removed_ if the template being applied includes only  tabs positioned via anchor text for the recipient, and none of the documents include the anchor text. When **true,** the recipients _will be preserved_ after the template is applied.")] string preserveTemplateRecipient = "", [OSParameter(Description = "Set to true to send the preserve_template_recipient query parameter to the API.")] bool includePreserveTemplateRecipient = false);

    [OSAction(Description = "Deletes a template from a document in an existing envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] TemplatesDeleteDocumentTemplates([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The ID of the template.")] string templateId);

    [OSAction(Description = "Gets the email setting overrides for an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EmailSettings EmailSettingsGetEmailSettings([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId);

    [OSAction(Description = "Updates the email setting overrides for an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EmailSettings EmailSettingsPutEmailSettings([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] EmailSettings requestBody);

    [OSAction(Description = "Adds email setting overrides to an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EmailSettings EmailSettingsPostEmailSettings([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] EmailSettings requestBody);

    [OSAction(Description = "Deletes the email setting overrides for an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EmailSettings EmailSettingsDeleteEmailSettings([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId);

    [OSAction(Description = "Returns envelope tab data for an existing envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeFormData FormDataGetFormData([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId);

    [OSAction(Description = "Gets the Original HTML Definition used to generate the Responsive HTML for the envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DocumentHtmlDefinitionOriginals ResponsiveHtmlGetEnvelopeHtmlDefinitions([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId);

    [OSAction(Description = "Gets envelope lock information.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeLocks LockGetEnvelopeLock([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId);

    [OSAction(Description = "Updates an envelope lock.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeLocks LockPutEnvelopeLock([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] LockRequest requestBody);

    [OSAction(Description = "Locks an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeLocks LockPostEnvelopeLock([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] LockRequest requestBody);

    [OSAction(Description = "Deletes an envelope lock.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeLocks LockDeleteEnvelopeLock([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId);

    [OSAction(Description = "Gets envelope notification information.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Notification NotificationGetEnvelopesEnvelopeIdNotification([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId);

    [OSAction(Description = "Sets envelope notifications for an existing envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Notification NotificationPutEnvelopesEnvelopeIdNotification([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeNotificationRequest requestBody);

    [OSAction(Description = "Gets the status of recipients for an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeRecipients RecipientsGetRecipients([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "When **true** and `include_tabs` value is set to **true,** all tabs with anchor tab properties are included in the response.")] string includeAnchorTabLocations = "", [OSParameter(Description = "Set to true to send the include_anchor_tab_locations query parameter to the API.")] bool includeIncludeAnchorTabLocations = false, [OSParameter(Description = "When **true,** the extended properties are included in the response.")] string includeExtended = "", [OSParameter(Description = "Set to true to send the include_extended query parameter to the API.")] bool includeIncludeExtended = false, [OSParameter(Description = "Boolean value that specifies whether to include metadata associated with the recipients (for envelopes only, not templates).")] string includeMetadata = "", [OSParameter(Description = "Set to true to send the include_metadata query parameter to the API.")] bool includeIncludeMetadata = false, [OSParameter(Description = "When **true,** the tab information associated with the recipient is included in the response.")] string includeTabs = "", [OSParameter(Description = "Set to true to send the include_tabs query parameter to the API.")] bool includeIncludeTabs = false);

    [OSAction(Description = "Updates recipients in a draft envelope or corrects recipient information for an in-process envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    RecipientsUpdateSummary RecipientsPutRecipients([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeRecipients requestBody, [OSParameter(Description = "When **true,** recipients are combined or merged with matching recipients. Recipient matching occurs as part of [template matching](https://support.docusign.com/s/document-item?bundleId=jux1643235969954&topicId=fxo1578456612662.html), and is based on Recipient Role and Routing Order.")] string combineSameOrderRecipients = "", [OSParameter(Description = "Set to true to send the combine_same_order_recipients query parameter to the API.")] bool includeCombineSameOrderRecipients = false, [OSParameter(Description = "Indicates if offline signing is enabled for the recipient when a network connection is unavailable.")] string offlineSigning = "", [OSParameter(Description = "Set to true to send the offline_signing query parameter to the API.")] bool includeOfflineSigning = false, [OSParameter(Description = "When **true,** forces the envelope to be resent if it would not be resent otherwise. Ordinarily, if the recipient's routing order is before or the same as the envelope's next recipient, the envelope is not resent. Setting this query parameter to **false** has no effect and is the same as omitting it altogether.")] string resendEnvelope = "", [OSParameter(Description = "Set to true to send the resend_envelope query parameter to the API.")] bool includeResendEnvelope = false);

    [OSAction(Description = "Adds one or more recipients to an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeRecipients RecipientsPostRecipients([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeRecipients requestBody, [OSParameter(Description = "When **true,** forces the envelope to be resent if it would not be resent otherwise. Ordinarily, if the recipient's routing order is before or the same as the envelope's next recipient, the envelope is not resent. Setting this query parameter to **false** has no effect and is the same as omitting it altogether.")] string resendEnvelope = "", [OSParameter(Description = "Set to true to send the resend_envelope query parameter to the API.")] bool includeResendEnvelope = false);

    [OSAction(Description = "Deletes recipients from an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeRecipients RecipientsDeleteRecipients([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeRecipients requestBody);

    [OSAction(Description = "Deletes a recipient from an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeRecipients RecipientsDeleteRecipient([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId);

    [OSAction(Description = "Gets the default Electronic Record and Signature Disclosure for an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ConsumerDisclosure ConsumerDisclosureGetConsumerDisclosureEn_4df2ebf1([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId, [OSParameter(Description = "(Optional) The code for the signer language version of the disclosure that you want to retrieve. The following languages are supported: - Arabic (`ar`) - Bulgarian (`bg`) - Czech (`cs`) - Chinese Simplified (`zh_CN`) - Chinese Traditional (`zh_TW`) - Croatian (`hr`) - Danish (`da`) - Dutch (`nl`) - English US (`en`) - English UK (`en_GB`) - Estonian (`et`) - Farsi (`fa`) - Finnish (`fi`) - French (`fr`) - French Canadian (`fr_CA`) - German (`de`) - Greek (`el`) - Hebrew (`he`) - Hindi (`hi`) - Hungarian (`hu`) - Bahasa Indonesian (`id`) - Italian (`it`) - Japanese (`ja`) - Korean (`ko`) - Latvian (`lv`) - Lithuanian (`lt`) - Bahasa Melayu (`ms`) - Norwegian (`no`) - Polish (`pl`) - Portuguese (`pt`) - Portuguese Brazil (`pt_BR`) - Romanian (`ro`) - Russian (`ru`) - Serbian (`sr`) - Slovak (`sk`) - Slovenian (`sl`) - Spanish (`es`) - Spanish Latin America (`es_MX`) - Swedish (`sv`) - Thai (`th`) - Turkish (`tr`) - Ukrainian (`uk`) - Vietnamese (`vi`) Additionally, you can automatically detect the browser language being used by the viewer and display the disclosure in that language by setting the value to `browser`.")] string langCode = "", [OSParameter(Description = "Set to true to send the langCode query parameter to the API.")] bool includeLangCode = false);

    [OSAction(Description = "Gets the Electronic Record and Signature Disclosure for a specific envelope recipient.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ConsumerDisclosure ConsumerDisclosureGetConsumerDisclosureEn_1d97a7ba([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "(Optional) The code for the signer language version of the disclosure that you want to retrieve, as a path parameter. The following languages are supported: - Arabic (`ar`) - Bulgarian (`bg`) - Czech (`cs`) - Chinese Simplified (`zh_CN`) - Chinese Traditional (`zh_TW`) - Croatian (`hr`) - Danish (`da`) - Dutch (`nl`) - English US (`en`) - English UK (`en_GB`) - Estonian (`et`) - Farsi (`fa`) - Finnish (`fi`) - French (`fr`) - French Canadian (`fr_CA`) - German (`de`) - Greek (`el`) - Hebrew (`he`) - Hindi (`hi`) - Hungarian (`hu`) - Bahasa Indonesian (`id`) - Italian (`it`) - Japanese (`ja`) - Korean (`ko`) - Latvian (`lv`) - Lithuanian (`lt`) - Bahasa Melayu (`ms`) - Norwegian (`no`) - Polish (`pl`) - Portuguese (`pt`) - Portuguese Brazil (`pt_BR`) - Romanian (`ro`) - Russian (`ru`) - Serbian (`sr`) - Slovak (`sk`) - Slovenian (`sl`) - Spanish (`es`) - Spanish Latin America (`es_MX`) - Swedish (`sv`) - Thai (`th`) - Turkish (`tr`) - Ukrainian (`uk`) - Vietnamese (`vi`) Additionally, you can automatically detect the browser language being used by the viewer and display the disclosure in that language by setting the value to `browser`.")] string langCode, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId, [OSParameter(Description = "(Optional) The code for the signer language version of the disclosure that you want to retrieve, as a query parameter. The following languages are supported: - Arabic (`ar`) - Bulgarian (`bg`) - Czech (`cs`) - Chinese Simplified (`zh_CN`) - Chinese Traditional (`zh_TW`) - Croatian (`hr`) - Danish (`da`) - Dutch (`nl`) - English US (`en`) - English UK (`en_GB`) - Estonian (`et`) - Farsi (`fa`) - Finnish (`fi`) - French (`fr`) - French Canadian (`fr_CA`) - German (`de`) - Greek (`el`) - Hebrew (`he`) - Hindi (`hi`) - Hungarian (`hu`) - Bahasa Indonesian (`id`) - Italian (`it`) - Japanese (`ja`) - Korean (`ko`) - Latvian (`lv`) - Lithuanian (`lt`) - Bahasa Melayu (`ms`) - Norwegian (`no`) - Polish (`pl`) - Portuguese (`pt`) - Portuguese Brazil (`pt_BR`) - Romanian (`ro`) - Russian (`ru`) - Serbian (`sr`) - Slovak (`sk`) - Slovenian (`sl`) - Spanish (`es`) - Spanish Latin America (`es_MX`) - Swedish (`sv`) - Thai (`th`) - Turkish (`tr`) - Ukrainian (`uk`) - Vietnamese (`vi`) Additionally, you can automatically detect the browser language being used by the viewer and display the disclosure in that language by setting the value to `browser`.", OriginalName = "langCode")] string langCode2 = "", [OSParameter(Description = "Set to true to send the langCode query parameter to the API.")] bool includeLangCode = false);

    [OSAction(Description = "Returns document visibility for a recipient", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DocumentVisibilityList RecipientsGetRecipientDocumentVisibility([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId);

    [OSAction(Description = "Updates document visibility for a recipient", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DocumentVisibilityList RecipientsPutRecipientDocumentVisibility([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId, [OSParameter(Description = "The JSON request body payload.")] DocumentVisibilityList requestBody);

    [OSAction(Description = "Creates a resource token for a sender to request ID Evidence data.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    IdEvidenceResourceToken RecipientsPostRecipientProofFileResourceToken([OSParameter(Description = "The account ID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The `recipientIdGuid`.")] string recipientId, [OSParameter(Description = "The token_scopes query parameter.")] string tokenScopes = "", [OSParameter(Description = "Set to true to send the token_scopes query parameter to the API.")] bool includeTokenScopes = false);

    [OSAction(Description = "Gets the initials image for a user.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] RecipientsGetRecipientInitialsImage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId, [OSParameter(Description = "Obsolete. The chrome is included in the image if it's present in the envelope.")] string includeChrome = "", [OSParameter(Description = "Set to true to send the include_chrome query parameter to the API.")] bool includeIncludeChrome = false);

    [OSAction(Description = "Sets the initials image for an accountless signer.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] RecipientsPutRecipientInitialsImage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId);

    [OSAction(Description = "Gets signature information for a signer or sign-in-person recipient.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserSignature RecipientsGetRecipientSignature([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId);

    [OSAction(Description = "Retrieve signature image information for a signer/sign-in-person recipient.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] RecipientsGetRecipientSignatureImage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId, [OSParameter(Description = "Obsolete. The chrome is included in the image if it's present in the envelope.")] string includeChrome = "", [OSParameter(Description = "Set to true to send the include_chrome query parameter to the API.")] bool includeIncludeChrome = false);

    [OSAction(Description = "Sets the signature image for an accountless signer.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] RecipientsPutRecipientSignatureImage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId);

    [OSAction(Description = "Gets the tabs information for a signer or sign-in-person recipient in an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeRecipientTabs RecipientsGetRecipientTabs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId, [OSParameter(Description = "When **true,** all tabs with anchor tab properties are included in the response. The default value is **false.**")] string includeAnchorTabLocations = "", [OSParameter(Description = "Set to true to send the include_anchor_tab_locations query parameter to the API.")] bool includeIncludeAnchorTabLocations = false, [OSParameter(Description = "When **true,** the response includes metadata indicating which properties are editable.")] string includeMetadata = "", [OSParameter(Description = "Set to true to send the include_metadata query parameter to the API.")] bool includeIncludeMetadata = false);

    [OSAction(Description = "Updates the tabs for a recipient.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeRecipientTabs RecipientsPutRecipientTabs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeRecipientTabs requestBody);

    [OSAction(Description = "Adds tabs for a recipient.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeRecipientTabs RecipientsPostRecipientTabs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeRecipientTabs requestBody);

    [OSAction(Description = "Deletes the tabs associated with a recipient. **Note:** It is an error to delete a tab that has the `templateLocked` property set to true. This property corresponds to the **Restrict changes** option in the web app.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeRecipientTabs RecipientsDeleteRecipientTabs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeRecipientTabs requestBody);

    [OSAction(Description = "Create the link to the page for manually reviewing IDs.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ViewUrl ViewsPostRecipientManualReviewView([OSParameter(Description = "A value that identifies your account. This value is automatically generated by Docusign for any account you create. Copy the value from the API Account ID field in the [AppsI and Keys](https://support.docusign.com/s/document-item?bundleId=pik1583277475390&topicId=pmp1583277397015.html) page.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "A GUID value that Docusign assigns to identify each recipient in an envelope. This value is globally unique for all recipients, not just those in your account. The specified recipient must belong to a workflow that allows the [manual review](https://support.docusign.com/s/document-item?bundleId=pik1583277475390&topicId=eya1583277454804.html) of IDs. In addition, the status of the automatic verification for this recipient must return `Failed` and the value of the `vendorFailureStatusCode` field must be `MANUAL_REVIEW_STARTED` as shown in the following extract of a response to the [GET ENVELOPE](/docs/esign-rest-api/reference/envelopes/envelopes/get/) method: <p> ``` \"recipientAuthenticationStatus\": { \"identityVerificationResult\": { \"status\": \"Failed\", \"eventTimestamp\": \"2020-09-04T16:59:42.8045667Z\", \"vendorFailureStatusCode\": \"MANUAL_REVIEW_STARTED\" } } ```")] string recipientId);

    [OSAction(Description = "Updates document visibility for recipients", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DocumentVisibilityList RecipientsPutRecipientsDocumentVisibility([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] DocumentVisibilityList requestBody);

    [OSAction(Description = "Creates a preview of the responsive versions of all of the documents in an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DocumentHtmlDefinitions ResponsiveHtmlPostResponsiveHtmlPreview([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] DocumentHtmlDefinition requestBody);

    [OSAction(Description = "", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopesSharesResponse EnvelopesSharesPostEnvelopesShares([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] EnvelopesSharesRequest requestBody);

    [OSAction(Description = "Reserved for Docusign.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] TabsBlobGetTabsBlob([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId);

    [OSAction(Description = "Reserved for Docusign.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] TabsBlobPutTabsBlob([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId);

    [OSAction(Description = "Gets templates used in an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    TemplateInformation TemplatesGetEnvelopeTemplates([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "Filters the results by template type. Valid values: * `applied`: Returns the templates applied to an envelope. * `matching`: Returns the [matching templates](https://support.docusign.com/s/document-item?language=en_US&bundleId=jux1643235969954&topicId=far1578456612069.html&_LANG=enus) for an envelope. The default value is `applied`.")] string include = "", [OSParameter(Description = "Set to true to send the include query parameter to the API.")] bool includeInclude = false);

    [OSAction(Description = "Adds templates to an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DocumentTemplateList TemplatesPostEnvelopeTemplates([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] DocumentTemplateList requestBody, [OSParameter(Description = "If omitted or set to false (the default), envelope recipients _will be removed_ if the template being applied includes only  tabs positioned via anchor text for the recipient, and none of the documents include the anchor text. When **true,** the recipients _will be preserved_ after the template is applied.")] string preserveTemplateRecipient = "", [OSParameter(Description = "Set to true to send the preserve_template_recipient query parameter to the API.")] bool includePreserveTemplateRecipient = false);

    [OSAction(Description = "Returns a URL to the envelope correction UI. Use after an envelope has been sent.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeViews ViewsPostEnvelopeCorrectView([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeViewRequest requestBody);

    [OSAction(Description = "Revokes the correction view URL to the Envelope UI.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] ViewsDeleteEnvelopeCorrectView([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] CorrectViewRequest requestBody);

    [OSAction(Description = "Returns a URL to the edit view UI. Use before an envelope has been sent.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeViews ViewsPostEnvelopeEditView([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeViewRequest requestBody);

    [OSAction(Description = "Returns a URL to the recipient view UI. For signer recipients, returns the embedded signing view. Can also be used for other recipient types.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeViews ViewsPostEnvelopeRecipientView([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the draft envelope or template to preview.")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] RecipientViewRequest requestBody);

    [OSAction(Description = "Creates an envelope recipient preview.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ViewUrl ViewsPostEnvelopeRecipientPreview([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] RecipientPreviewRequest requestBody);

    [OSAction(Description = "Returns a URL to the sender view UI. Used before an envelope has been sent.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeViews ViewsPostEnvelopeSenderView([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeViewRequest requestBody);

    [OSAction(Description = "Returns a URL to the shared recipient view UI for an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ViewUrl ViewsPostEnvelopeRecipientSharedView([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] RecipientViewRequest requestBody);

    [OSAction(Description = "Returns the workflow definition for an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Workflow EnvelopeWorkflowDefinitionV2GetEnvelopeWo_ca403e5e([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId);

    [OSAction(Description = "Updates the workflow definition for an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Workflow EnvelopeWorkflowDefinitionV2PutEnvelopeWo_91343534([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] Workflow requestBody);

    [OSAction(Description = "Delete the workflow definition for an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] EnvelopeWorkflowDefinitionV2DeleteEnvelop_21f585c7([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId);

    [OSAction(Description = "Returns the scheduled sending rules for an envelope's workflow definition.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ScheduledSending EnvelopeWorkflowScheduledSendingGetEnvelo_5f6bd7d7([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId);

    [OSAction(Description = "Updates the scheduled sending rules for an envelope's workflow.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ScheduledSending EnvelopeWorkflowScheduledSendingPutEnvelo_ae0bb984([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] ScheduledSending requestBody);

    [OSAction(Description = "Deletes the scheduled sending rules for the envelope's workflow.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] EnvelopeWorkflowScheduledSendingDeleteEnv_a15e6531([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId);

    [OSAction(Description = "Adds a new step to an envelope's workflow.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    WorkflowStep EnvelopeWorkflowStepPostEnvelopeWorkflowS_4ee02cd5([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The JSON request body payload.")] WorkflowStep requestBody);

    [OSAction(Description = "Returns a specified workflow step for a specified template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    WorkflowStep EnvelopeWorkflowStepGetEnvelopeWorkflowSt_26375df7([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The ID of the workflow step.")] string workflowStepId);

    [OSAction(Description = "Updates the specified workflow step for an envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    WorkflowStep EnvelopeWorkflowStepPutEnvelopeWorkflowSt_7129e3b9([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The ID of the workflow step.")] string workflowStepId, [OSParameter(Description = "The JSON request body payload.")] WorkflowStep requestBody);

    [OSAction(Description = "Deletes a workflow step from an envelope's workflow definition.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] EnvelopeWorkflowStepDeleteEnvelopeWorkflo_c04fee5e([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The ID of the workflow step.")] string workflowStepId);

    [OSAction(Description = "Returns the delayed routing rules for an envelope's workflow step definition.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DelayedRouting EnvelopeWorkflowDelayedRoutingGetEnvelope_4a07658d([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The ID of the workflow step.")] string workflowStepId);

    [OSAction(Description = "Updates the delayed routing rules for an envelope's workflow step definition.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DelayedRouting EnvelopeWorkflowDelayedRoutingPutEnvelope_8192b9fd([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The ID of the workflow step.")] string workflowStepId, [OSParameter(Description = "The JSON request body payload.")] DelayedRouting requestBody);

    [OSAction(Description = "Deletes the delayed routing rules for the specified envelope workflow step.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] EnvelopeWorkflowDelayedRoutingDeleteEnvel_9260c986([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The envelope's GUID. Example: `93be49ab-xxxx-xxxx-xxxx-f752070d71ec`")] string envelopeId, [OSParameter(Description = "The ID of the workflow step.")] string workflowStepId);

    [OSAction(Description = "Gets envelope statuses for a set of envelopes.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopesInformation EnvelopesPutStatus([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeIdsRequest requestBody, [OSParameter(Description = "Specifies the Authoritative Copy Status for the envelopes. Valid values: - `Unknown` - `Original` - `Transferred` - `AuthoritativeCopy` - `AuthoritativeCopyExportPending` - `AuthoritativeCopyExported` - `DepositPending` - `Deposited` - `DepositedEO` - `DepositFailed`")] string acStatus = "", [OSParameter(Description = "Set to true to send the ac_status query parameter to the API.")] bool includeAcStatus = false, [OSParameter(Description = "When **true,** removes any results that match one of the provided `transaction_ids`.")] string block = "", [OSParameter(Description = "Set to true to send the block query parameter to the API.")] bool includeBlock = false, [OSParameter(Description = "The maximum number of results to return. Use `start_position` to specify the number of results to skip.")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "The email address of the sender.")] string email = "", [OSParameter(Description = "Set to true to send the email query parameter to the API.")] bool includeEmail = false, [OSParameter(Description = "The envelope IDs to include in the results. The value of this property can be: * For the `GET` implementation of this method, use a comma-separated list of envelope IDs. * For the `PUT` implementation of this method, use the `request_body` value, and include the envelope IDs in the request body.")] string envelopeIds = "", [OSParameter(Description = "Set to true to send the envelope_ids query parameter to the API.")] bool includeEnvelopeIds = false, [OSParameter(Description = "The date/time setting that specifies when the request begins checking for status changes for envelopes in the account. This is required unless parameters `envelope_ids` and/or `transaction_Ids` are provided. **Note:** This parameter must be set to a valid  `DateTime`, or  `envelope_ids` and/or `transaction_ids` must be specified.")] string fromDate = "", [OSParameter(Description = "Set to true to send the from_date query parameter to the API.")] bool includeFromDate = false, [OSParameter(Description = "The envelope status that you are checking for. Possible values are: - `Changed` (default) - `Completed` - `Created` - `Declined` - `Deleted` - `Delivered` - `Processing` - `Sent` - `Signed` - `TimedOut` - `Voided` For example, if you specify `Changed`, this method returns a list of envelopes that changed status during the `from_date` to `to_date` time period.")] string fromToStatus = "", [OSParameter(Description = "Set to true to send the from_to_status query parameter to the API.")] bool includeFromToStatus = false, [OSParameter(Description = "The zero-based index of the result from which to start returning results. Use with `count` to limit the number of results. The default value is `0`.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false, [OSParameter(Description = "A comma-separated list of envelope status to search for. Possible values are: - `completed` - `created` - `declined` - `deleted` - `delivered` - `processing` - `sent` - `signed` - `template` - `voided`")] string status = "", [OSParameter(Description = "Set to true to send the status query parameter to the API.")] bool includeStatus = false, [OSParameter(Description = "Optional date/time setting that specifies the last date/time or envelope status changes in the result set. The default value is the time that you call the method.")] string toDate = "", [OSParameter(Description = "Set to true to send the to_date query parameter to the API.")] bool includeToDate = false, [OSParameter(Description = "The transaction IDs to include in the results. Note that transaction IDs are valid for seven days. The value of this property can be: - A list of comma-separated transaction IDs - The special value `request_body`. In this case, this method uses the transaction IDs in the request body.")] string transactionIds = "", [OSParameter(Description = "Set to true to send the transaction_ids query parameter to the API.")] bool includeTransactionIds = false, [OSParameter(Description = "Limits results to envelopes sent by the account user with this user name. `email` must be given as well, and both `email` and `user_name` must refer to an existing account user.")] string userName = "", [OSParameter(Description = "Set to true to send the user_name query parameter to the API.")] bool includeUserName = false);

    [OSAction(Description = "Gets envelope transfer rules.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeTransferRuleInformation EnvelopeTransferRulesGetEnvelopeTransferRules([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The maximum number of results to return. Use `start_position` to specify the number of results to skip.")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "The zero-based index of the result from which to start returning results. Use with `count` to limit the number of results. The default value is `0`.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false);

    [OSAction(Description = "Changes the status of multiple envelope transfer rules.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeTransferRuleInformation EnvelopeTransferRulesPutEnvelopeTransferRules([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeTransferRuleInformation requestBody);

    [OSAction(Description = "Creates an envelope transfer rule.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeTransferRuleInformation EnvelopeTransferRulesPostEnvelopeTransferRules([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeTransferRuleRequest requestBody);

    [OSAction(Description = "Changes the status of an envelope transfer rule.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeTransferRule EnvelopeTransferRulesPutEnvelopeTransferRule([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the envelope transfer rule. The system generates this ID when the rule is first created.")] string envelopeTransferRuleId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeTransferRule requestBody);

    [OSAction(Description = "Deletes an envelope transfer rule.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] EnvelopeTransferRulesDeleteEnvelopeTransferRules([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the envelope transfer rule. The system generates this ID when the rule is first created.")] string envelopeTransferRuleId);

    [OSAction(Description = "Retrieves the list of favorite templates for the account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    FavoriteTemplatesInfo FavoriteTemplatesGetFavoriteTemplates([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Set one or more templates as account favorites.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    FavoriteTemplatesInfo FavoriteTemplatesPutFavoriteTemplate([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] FavoriteTemplatesInfo requestBody);

    [OSAction(Description = "Remove one or more templates from the account favorites.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    FavoriteTemplatesInfo FavoriteTemplatesUnFavoriteTemplate([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] FavoriteTemplatesInfo requestBody);

    [OSAction(Description = "Returns a list of the account's folders.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    FoldersResponse FoldersGetFolders([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The maximum number of results to return.")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "A comma-separated list of folder types to include in the response. Valid values are: - `envelope_folders`: Returns a list of envelope folders. (Default) - `template_folders`: Returns a list of template folders. - `shared_template_folders`: Returns a list of shared template folders.")] string include = "", [OSParameter(Description = "Set to true to send the include query parameter to the API.")] bool includeInclude = false, [OSParameter(Description = "Indicates whether folder items are included in the response. If this parameter is omitted, the default is false.")] string includeItems = "", [OSParameter(Description = "Set to true to send the include_items query parameter to the API.")] bool includeIncludeItems = false, [OSParameter(Description = "The zero-based index of the result from which to start returning results. The default value is `0`.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false, [OSParameter(Description = "If missing or any value other than `-1`, the returned list contains only the top-level folders. A value of `-1` returns the complete folder hierarchy.")] string subFolderDepth = "", [OSParameter(Description = "Set to true to send the sub_folder_depth query parameter to the API.")] bool includeSubFolderDepth = false, [OSParameter(Description = "This parameter is deprecated as of version 2.1. Use `include` instead.")] string template = "", [OSParameter(Description = "Set to true to send the template query parameter to the API.")] bool includeTemplate = false, [OSParameter(Description = "Narrows down the resulting folder list by the following values: - `all`: Returns all templates owned or shared with the user. (default) - `owned_by_me`: Returns only  templates the user owns. - `shared_with_me`: Returns only templates that are shared with the user.")] string userFilter = "", [OSParameter(Description = "Set to true to send the user_filter query parameter to the API.")] bool includeUserFilter = false);

    [OSAction(Description = "Gets information about items in a specified folder.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    FolderItemsResponse FoldersGetFolderItems([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the folder.")] string folderId, [OSParameter(Description = "Reserved for Docusign.")] string fromDate = "", [OSParameter(Description = "Set to true to send the from_date query parameter to the API.")] bool includeFromDate = false, [OSParameter(Description = "Indicates whether folder items are included in the response. If this parameter is omitted, the default is false.")] string includeItems = "", [OSParameter(Description = "Set to true to send the include_items query parameter to the API.")] bool includeIncludeItems = false, [OSParameter(Description = "Reserved for Docusign.")] string ownerEmail = "", [OSParameter(Description = "Set to true to send the owner_email query parameter to the API.")] bool includeOwnerEmail = false, [OSParameter(Description = "Reserved for Docusign.")] string ownerName = "", [OSParameter(Description = "Set to true to send the owner_name query parameter to the API.")] bool includeOwnerName = false, [OSParameter(Description = "Reserved for Docusign.")] string searchText = "", [OSParameter(Description = "Set to true to send the search_text query parameter to the API.")] bool includeSearchText = false, [OSParameter(Description = "Reserved for Docusign.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false, [OSParameter(Description = "Reserved for Docusign.")] string status = "", [OSParameter(Description = "Set to true to send the status query parameter to the API.")] bool includeStatus = false, [OSParameter(Description = "Reserved for Docusign.")] string toDate = "", [OSParameter(Description = "Set to true to send the to_date query parameter to the API.")] bool includeToDate = false);

    [OSAction(Description = "Moves a set of envelopes from their current folder to another folder.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    FoldersResponse FoldersPutFolderById([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the folder.")] string folderId, [OSParameter(Description = "The JSON request body payload.")] FoldersRequest requestBody);

    [OSAction(Description = "Gets information about groups associated with the account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    GroupInformation GroupsGetGroups([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The maximum number of results to return. Use `start_position` to specify the number of results to skip. Valid values: `1` to `100`")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "The type of group to return. Valid values: * `AdminGroup` * `CustomGroup` * `EveryoneGroup`")] string groupType = "", [OSParameter(Description = "Set to true to send the group_type query parameter to the API.")] bool includeGroupType = false, [OSParameter(Description = "When **true,** every group returned in the response includes a `userCount` property that contains the total number of users in the group. The default is **true.**")] string includeUsercount = "", [OSParameter(Description = "Set to true to send the include_usercount query parameter to the API.")] bool includeIncludeUsercount = false, [OSParameter(Description = "Filters the results of a GET request based on the text that you specify.")] string searchText = "", [OSParameter(Description = "Set to true to send the search_text query parameter to the API.")] bool includeSearchText = false, [OSParameter(Description = "The zero-based index of the result from which to start returning results. Use with `count` to limit the number of results. The default value is `0`.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false);

    [OSAction(Description = "Updates the group information for a group.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    GroupInformation GroupsPutGroups([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] GroupInformation requestBody);

    [OSAction(Description = "Creates one or more groups for the account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    GroupInformation GroupsPostGroups([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] GroupInformation requestBody);

    [OSAction(Description = "Deletes an existing user group.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    GroupInformation GroupsDeleteGroups([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] GroupInformation requestBody);

    [OSAction(Description = "Gets the brand information for a group.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    GroupBrands BrandsGetGroupBrands([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the group.")] string groupId);

    [OSAction(Description = "Adds an existing brand to a group.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    GroupBrands BrandsPutGroupBrands([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the group being accessed.")] string groupId, [OSParameter(Description = "The JSON request body payload.")] BrandsRequest requestBody);

    [OSAction(Description = "Deletes brand information from a group.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    GroupBrands BrandsDeleteGroupBrands([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the group.")] string groupId, [OSParameter(Description = "The JSON request body payload.")] BrandsRequest requestBody);

    [OSAction(Description = "Gets a list of users in a group.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UsersResponse GroupsGetGroupUsers([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the group being accessed.")] string groupId, [OSParameter(Description = "The maximum number of results to return. Use `start_position` to specify the number of results to skip. Valid values: `1` to `100`<br> Default: `50`")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "The zero-based index of the result from which to start returning results. Use with `count` to limit the number of results. The default value is `0`.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false);

    [OSAction(Description = "Adds one or more users to an existing group.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UsersResponse GroupsPutGroupUsers([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the group being accessed.")] string groupId, [OSParameter(Description = "The JSON request body payload.")] UserInfoList requestBody);

    [OSAction(Description = "Deletes one or more users from a group", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UsersResponse GroupsDeleteGroupUsers([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the group being accessed.")] string groupId, [OSParameter(Description = "The JSON request body payload.")] UserInfoList requestBody);

    [OSAction(Description = "Retrieves the Identity Verification workflows available to an account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountIdentityVerificationResponse AccountIdentityVerificationGetAccountIden_717a34ef([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "Filters the workflows returned according to status. Valid values: - `active`: Only active workflows are returned. This is the default. - `deactivated`: Only deactivated workflows are returned. - `all`: All workflows are returned.")] string identityVerificationWorkflowStatus = "", [OSParameter(Description = "Set to true to send the identity_verification_workflow_status query parameter to the API.")] bool includeIdentityVerificationWorkflowStatus = false);

    [OSAction(Description = "List payment gateway accounts", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    PaymentGatewayAccountsInfo PaymentGatewayAccountsGetAllPaymentGatewayAccounts([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Gets a list of permission profiles.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    PermissionProfileInformation PermissionProfilesGetPermissionProfiles([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "A comma-separated list of additional properties to return in the response. Valid values are: - `user_count`: The total number of users associated with the permission profile. - `closed_users`: Includes closed users in the `user_count`. - `account_management`: The account management settings. - `metadata`: Metadata indicating whether the properties associated with the account permission profile are editable. Example: `user_count,closed_users`")] string include = "", [OSParameter(Description = "Set to true to send the include query parameter to the API.")] bool includeInclude = false);

    [OSAction(Description = "Creates a new permission profile for an account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    PermissionProfile PermissionProfilesPostPermissionProfiles([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] PermissionProfile requestBody, [OSParameter(Description = "A comma-separated list of additional properties to return in the response. The only valid value for this request is `metadata`, which returns metadata indicating whether the properties associated with the account permission profile are editable.")] string include = "", [OSParameter(Description = "Set to true to send the include query parameter to the API.")] bool includeInclude = false);

    [OSAction(Description = "Returns a permission profile for an account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    PermissionProfile PermissionProfilesGetPermissionProfile([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the permission profile. Use [AccountPermissionProfiles: list](/docs/esign-rest-api/reference/accounts/accountpermissionprofiles/list/) to get a list of permission profiles and their IDs. You can also download a CSV file of all permission profiles and their IDs from the **Settings > Permission Profiles** page of your eSignature account page.")] string permissionProfileId, [OSParameter(Description = "A comma-separated list of additional properties to return in the response. The only valid value for this request is `metadata`, which returns metadata indicating whether the properties associated with the account permission profile are editable.")] string include = "", [OSParameter(Description = "Set to true to send the include query parameter to the API.")] bool includeInclude = false);

    [OSAction(Description = "Updates a permission profile.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    PermissionProfile PermissionProfilesPutPermissionProfiles([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the permission profile. Use [AccountPermissionProfiles: list](/docs/esign-rest-api/reference/accounts/accountpermissionprofiles/list/) to get a list of permission profiles and their IDs. You can also download a CSV file of all permission profiles and their IDs from the **Settings > Permission Profiles** page of your eSignature account page.")] string permissionProfileId, [OSParameter(Description = "The JSON request body payload.")] PermissionProfile requestBody, [OSParameter(Description = "A comma-separated list of additional properties to return in the response. The only valid value for this request is `metadata`, which returns metadata indicating whether the properties associated with the account permission profile are editable.")] string include = "", [OSParameter(Description = "Set to true to send the include query parameter to the API.")] bool includeInclude = false);

    [OSAction(Description = "Deletes a permission profile from an account.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] PermissionProfilesDeletePermissionProfiles([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the permission profile. Use [AccountPermissionProfiles: list](/docs/esign-rest-api/reference/accounts/accountpermissionprofiles/list/) to get a list of permission profiles and their IDs. You can also download a CSV file of all permission profiles and their IDs from the **Settings > Permission Profiles** page of your eSignature account page.")] string permissionProfileId, [OSParameter(Description = "The move_users_to query parameter.")] string moveUsersTo = "", [OSParameter(Description = "Set to true to send the move_users_to query parameter to the API.")] bool includeMoveUsersTo = false);

    [OSAction(Description = "Returns a list of PowerForms.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    PowerFormsResponse PowerFormsGetPowerFormsList([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The maximum number of results to return.")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "The start date for a date range. **Note:** If no value is provided, no date filtering is applied.")] string fromDate = "", [OSParameter(Description = "Set to true to send the from_date query parameter to the API.")] bool includeFromDate = false, [OSParameter(Description = "The order in which to sort the results. Valid values are: * `asc`: Ascending order. * `desc`: Descending order.")] string order = "", [OSParameter(Description = "Set to true to send the order query parameter to the API.")] bool includeOrder = false, [OSParameter(Description = "The file attribute to use to sort the results. Valid values are: - `sender` - `auth` - `used` - `remaining` - `lastused` - `status` - `type` - `templatename` - `created`")] string orderBy = "", [OSParameter(Description = "Set to true to send the order_by query parameter to the API.")] bool includeOrderBy = false, [OSParameter(Description = "A comma-separated list of additional properties to include in a search. - `sender`: Include sender name and email in the search. - `recipients`: Include recipient names and emails in the search. - `envelope`: Include envelope information in the search.")] string searchFields = "", [OSParameter(Description = "Set to true to send the search_fields query parameter to the API.")] bool includeSearchFields = false, [OSParameter(Description = "Use this parameter to search for specific text.")] string searchText = "", [OSParameter(Description = "Set to true to send the search_text query parameter to the API.")] bool includeSearchText = false, [OSParameter(Description = "The position within the total result set from which to start returning values. The value **thumbnail** may be used to return the page image.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false, [OSParameter(Description = "The end date for a date range. **Note:** If no value is provided, this property defaults to the current date.")] string toDate = "", [OSParameter(Description = "Set to true to send the to_date query parameter to the API.")] bool includeToDate = false);

    [OSAction(Description = "Creates a new PowerForm", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    PowerForm PowerFormsPostPowerForm([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] PowerForm requestBody);

    [OSAction(Description = "Deletes one or more PowerForms.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    PowerFormsResponse PowerFormsDeletePowerFormsList([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] PowerFormsRequest requestBody);

    [OSAction(Description = "Returns a single PowerForm.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    PowerForm PowerFormsGetPowerForm([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the PowerForm.")] string powerFormId);

    [OSAction(Description = "Updates an existing PowerForm.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    PowerForm PowerFormsPutPowerForm([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the PowerForm.")] string powerFormId, [OSParameter(Description = "The JSON request body payload.")] PowerForm requestBody);

    [OSAction(Description = "Deletes a PowerForm.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] PowerFormsDeletePowerForm([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the PowerForm.")] string powerFormId);

    [OSAction(Description = "Returns the data that users entered in a PowerForm.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    PowerFormsFormDataResponse PowerFormsGetPowerFormFormData([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the PowerForm.")] string powerFormId, [OSParameter(Description = "The layout in which to return the PowerForm data. For each of the following layouts, set the `Accept` header to the corresponding value. Valid values are: - `Native` (Set `Accept` header to `application/json`) - `Csv_Classic` (Set `Accept` header to `application/csv`) - `Csv_One_Envelope_Per_Line` (Set `Accept` header to `text/csv`) - `Xml_Classic` (Set `Accept` header to `application/xml`)")] string dataLayout = "", [OSParameter(Description = "Set to true to send the data_layout query parameter to the API.")] bool includeDataLayout = false, [OSParameter(Description = "The start date for a date range in UTC DateTime format. **Note:** If this property is null, no date filtering is applied.")] string fromDate = "", [OSParameter(Description = "Set to true to send the from_date query parameter to the API.")] bool includeFromDate = false, [OSParameter(Description = "The end date of a date range in UTC DateTime format. The default value is `UtcNow`.")] string toDate = "", [OSParameter(Description = "Set to true to send the to_date query parameter to the API.")] bool includeToDate = false);

    [OSAction(Description = "Gets PowerForm senders.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    PowerFormSendersResponse PowerFormsGetPowerFormsSenders([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The position within the total result set from which to start returning values. The value **thumbnail** may be used to return the page image.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false);

    [OSAction(Description = "Gets the recipient names associated with an email address.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    RecipientNamesResponse RecipientNamesGetRecipientNames([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "(Required) The email address for which you want to retrieve recipient names.")] string email = "", [OSParameter(Description = "Set to true to send the email query parameter to the API.")] bool includeEmail = false);

    [OSAction(Description = "Returns available seals for specified account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountSealProviders AccountSignatureProvidersGetSealProviders([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Deprecated. Use Envelopes: listStatusChanges.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    FolderItemResponse SearchFoldersGetSearchFolderContents([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "Specifies the envelope group that is searched by the request. These are logical groupings, not actual folder names. Valid values are: drafts, awaiting_my_signature, completed, out_for_signature.")] string searchFolderId, [OSParameter(Description = "Specifies that all envelopes that match the criteria are returned.")] string all = "", [OSParameter(Description = "Set to true to send the all query parameter to the API.")] bool includeAll = false, [OSParameter(Description = "The maximum number of results to return. Use `start_position` to specify the number of results to skip. Valid values: `1` to `100`")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "Specifies the start of the date range to return. If no value is provided, the default search is the previous 30 days.")] string fromDate = "", [OSParameter(Description = "Set to true to send the from_date query parameter to the API.")] bool includeFromDate = false, [OSParameter(Description = "When **true,** the recipient information is returned in the response.")] string includeRecipients = "", [OSParameter(Description = "Set to true to send the include_recipients query parameter to the API.")] bool includeIncludeRecipients = false, [OSParameter(Description = "Specifies the order in which the list is returned. Valid values are: `asc` for ascending order, and `desc` for descending order.")] string order = "", [OSParameter(Description = "Set to true to send the order query parameter to the API.")] bool includeOrder = false, [OSParameter(Description = "Specifies the property used to sort the list. Valid values are: `action_required`, `created`, `completed`, `sent`, `signer_list`, `status`, or `subject`.")] string orderBy = "", [OSParameter(Description = "Set to true to send the order_by query parameter to the API.")] bool includeOrderBy = false, [OSParameter(Description = "The zero-based index of the result from which to start returning results. Use with `count` to limit the number of results. The default value is `0`.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false, [OSParameter(Description = "Specifies the end of the date range to return.")] string toDate = "", [OSParameter(Description = "Set to true to send the to_date query parameter to the API.")] bool includeToDate = false);

    [OSAction(Description = "Gets account settings information.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountSettingsInformation SettingsGetSettings([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Updates the account settings for an account.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] SettingsPutSettings([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] AccountSettingsInformation requestBody);

    [OSAction(Description = "Gets the BCC email archive configurations for an account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BccEmailArchiveList BCCEmailArchiveGetBCCEmailArchiveList([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The maximum number of results to return. Use `start_position` to specify the number of results to skip.")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "The zero-based index of the result from which to start returning results. Use with `count` to limit the number of results. The default value is `0`.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false);

    [OSAction(Description = "Creates a BCC email archive configuration.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BccEmailArchive BCCEmailArchivePostBCCEmailArchive([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] BccEmailArchive requestBody);

    [OSAction(Description = "Gets a BCC email archive configuration and its history.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BccEmailArchiveHistoryList BCCEmailArchiveGetBCCEmailArchiveHistoryList([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the BCC email archive configuration.")] string bccEmailArchiveId, [OSParameter(Description = "The maximum number of results to return. Use `start_position` to specify the number of items to skip.")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "The zero-based index of the result from which to start returning results. Use with `count` to limit the number of results. The default value is `0`.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false);

    [OSAction(Description = "Deletes a BCC email archive configuration.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] BCCEmailArchiveDeleteBCCEmailArchive([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the BCC email archive configuration.")] string bccEmailArchiveId);

    [OSAction(Description = "Returns the configuration information for the eNote eOriginal integration.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ENoteConfiguration ENoteConfigurationGetENoteConfiguration([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Updates configuration information for the eNote eOriginal integration.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ENoteConfiguration ENoteConfigurationPutENoteConfiguration([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] ENoteConfiguration requestBody);

    [OSAction(Description = "Deletes configuration information for the eNote eOriginal integration.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] ENoteConfigurationDeleteENoteConfiguration([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Gets the envelope purge configuration for an account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopePurgeConfiguration EnvelopePurgeConfigurationGetEnvelopePurg_9c571224([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Sets the envelope purge configuration for an account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopePurgeConfiguration EnvelopePurgeConfigurationPutEnvelopePurg_80450268([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] EnvelopePurgeConfiguration requestBody);

    [OSAction(Description = "Gets envelope notification defaults.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    NotificationDefaults NotificationDefaultsGetNotificationDefaults([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Updates envelope notification default settings.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    NotificationDefaults NotificationDefaultsPutNotificationDefaults([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] NotificationDefaults requestBody);

    [OSAction(Description = "Gets the password rules for an account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountPasswordRules AccountPasswordRulesGetAccountPasswordRules([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Updates the password rules for an account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountPasswordRules AccountPasswordRulesPutAccountPasswordRules([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] AccountPasswordRules requestBody);

    [OSAction(Description = "Returns tab settings list for specified account", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    TabAccountSettings TabSettingsGetTabSettings([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Modifies tab settings for specified account", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    TabAccountSettings TabSettingsPutSettings([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] TabAccountSettings requestBody);

    [OSAction(Description = "Reserved: Gets the shared item status for one or more users.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountSharedAccess SharedAccessGetSharedAccess([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The maximum number of results to return. Use `start_position` to specify the number of results to skip. Default: `1000`")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "This query parameter works in conjunction with `user_ids`. When you specify one of the following user statuses, the query limits the results to only users that match the specified status: - `ActivationRequired`: Membership Activation required - `ActivationSent`: Membership activation sent to user - `Active`: User Membership is active - `Closed`: User Membership is closed - `Disabled`: User Membership is disabled")] string envelopesNotSharedUserStatus = "", [OSParameter(Description = "Set to true to send the envelopes_not_shared_user_status query parameter to the API.")] bool includeEnvelopesNotSharedUserStatus = false, [OSParameter(Description = "A comma-separated list of folder IDs for which to return shared item information. If `item_type` is set to `folders`, at least one folder ID is required.")] string folderIds = "", [OSParameter(Description = "Set to true to send the folder_ids query parameter to the API.")] bool includeFolderIds = false, [OSParameter(Description = "Specifies the type of shared item being requested. Valid values: - `envelopes`: Get information about envelope sharing between users. - `templates`: Get information about template sharing among users and groups. - `folders`: Get information about folder sharing among users and groups.")] string itemType = "", [OSParameter(Description = "Set to true to send the item_type query parameter to the API.")] bool includeItemType = false, [OSParameter(Description = "Filter user names based on the specified string. The wild-card '*' (asterisk) can be used in the string.")] string searchText = "", [OSParameter(Description = "Set to true to send the search_text query parameter to the API.")] bool includeSearchText = false, [OSParameter(Description = "A comma-separated list of sharing filters that specifies which users appear in the response. - `not_shared`: The response lists users who do not share items of `item_type` with the current user. - `shared_to`: The response lists users in `user_list` who are sharing items to current user. - `shared_from`: The response lists users in `user_list` who are sharing items from the current user. - `shared_to_and_from`: The response lists users in `user_list` who are sharing items to and from the current user. If the current user does not have administrative privileges, only the `shared_to` option is valid.")] string shared = "", [OSParameter(Description = "Set to true to send the shared query parameter to the API.")] bool includeShared = false, [OSParameter(Description = "The zero-based index of the result from which to start returning results. Use with `count` to limit the number of results. The default value is `0`.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false, [OSParameter(Description = "A comma-separated list of user IDs for whom the shared item information is being requested.")] string userIds = "", [OSParameter(Description = "Set to true to send the user_ids query parameter to the API.")] bool includeUserIds = false);

    [OSAction(Description = "Reserved: Sets the shared access information for users.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountSharedAccess SharedAccessPutSharedAccess([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] AccountSharedAccess requestBody, [OSParameter(Description = "Specifies the type of shared item being set: - `envelopes`: Set envelope sharing between users. - `templates`: Set information about template sharing among users and groups. - `folders`: Get information about folder sharing among users and groups.")] string itemType = "", [OSParameter(Description = "Set to true to send the item_type query parameter to the API.")] bool includeItemType = false, [OSParameter(Description = "When **true,** preserve the existing shared access settings.")] string preserveExistingSharedAccess = "", [OSParameter(Description = "Set to true to send the preserve_existing_shared_access query parameter to the API.")] bool includePreserveExistingSharedAccess = false, [OSParameter(Description = "A comma-separated list of IDs for users whose shared item access is being set.")] string userIds = "", [OSParameter(Description = "Set to true to send the user_ids query parameter to the API.")] bool includeUserIds = false);

    [OSAction(Description = "Gets the available signature providers for an account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountSignatureProviders AccountSignatureProvidersGetSignatureProviders([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Returns a list of stamps available in the account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountSignaturesInformation AccountSignaturesGetAccountSignatures([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The format of the stamp to return. Valid values: - `NameDateHanko` - `NameHanko` - `PlaceholderHanko`")] string stampFormat = "", [OSParameter(Description = "Set to true to send the stamp_format query parameter to the API.")] bool includeStampFormat = false, [OSParameter(Description = "The name associated with the stamps to return. This value can be a Japanese surname (up to 5 characters) or a purchase order ID.")] string stampName = "", [OSParameter(Description = "Set to true to send the stamp_name query parameter to the API.")] bool includeStampName = false, [OSParameter(Description = "The type of the stamps to return. Valid values: - `name_stamp` - `stamp` - `signature`")] string stampType = "", [OSParameter(Description = "Set to true to send the stamp_type query parameter to the API.")] bool includeStampType = false);

    [OSAction(Description = "Updates an account stamp.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountSignaturesInformation AccountSignaturesPutAccountSignature([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] AccountSignaturesInformation requestBody);

    [OSAction(Description = "Adds or updates one or more account stamps.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountSignaturesInformation AccountSignaturesPostAccountSignatures([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] AccountSignaturesInformation requestBody, [OSParameter(Description = "The decode_only query parameter.")] string decodeOnly = "", [OSParameter(Description = "Set to true to send the decode_only query parameter to the API.")] bool includeDecodeOnly = false);

    [OSAction(Description = "Returns information about the specified stamp.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountSignature AccountSignaturesGetAccountSignature([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the account stamp.")] string signatureId);

    [OSAction(Description = "Updates an account stamp by ID.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountSignature AccountSignaturesPutAccountSignatureById([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the account stamp.")] string signatureId, [OSParameter(Description = "The JSON request body payload.")] AccountSignatureDefinition requestBody, [OSParameter(Description = "When **true,** closes the current signature.")] string closeExistingSignature = "", [OSParameter(Description = "Set to true to send the close_existing_signature query parameter to the API.")] bool includeCloseExistingSignature = false);

    [OSAction(Description = "Deletes an account stamp.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] AccountSignaturesDeleteAccountSignature([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the account stamp.")] string signatureId);

    [OSAction(Description = "Returns the image for an account stamp.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] AccountSignaturesGetAccountSignatureImage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "Specificies the type of image. Valid values: - `stamp_image` - `signature_image` - `initials_image`")] string imageType, [OSParameter(Description = "The ID of the account stamp.")] string signatureId, [OSParameter(Description = "When **true,** the chrome (or frame containing the added line and identifier) is included with the signature image.")] string includeChrome = "", [OSParameter(Description = "Set to true to send the include_chrome query parameter to the API.")] bool includeIncludeChrome = false);

    [OSAction(Description = "Sets a signature image, initials, or stamp.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountSignature AccountSignaturesPutAccountSignatureImage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "Specificies the type of image. Valid values: - `stamp_image` - `signature_image` - `initials_image`")] string imageType, [OSParameter(Description = "The ID of the account stamp.")] string signatureId, [OSParameter(Description = "The transparent_png query parameter.")] string transparentPng = "", [OSParameter(Description = "Set to true to send the transparent_png query parameter to the API.")] bool includeTransparentPng = false);

    [OSAction(Description = "Deletes the image for a stamp.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    AccountSignature AccountSignaturesDeleteAccountSignatureImage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "Specificies the type of image. Valid values: - `stamp_image` - `signature_image` - `initials_image`")] string imageType, [OSParameter(Description = "The ID of the account stamp.")] string signatureId);

    [OSAction(Description = "Gets a list of the Signing Groups in an account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    SigningGroupInformation SigningGroupsGetSigningGroups([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "Filters by the type of signing group. Valid values: * `sharedSigningGroup` * `privateSigningGroup` * `systemSigningGroup`")] string groupType = "", [OSParameter(Description = "Set to true to send the group_type query parameter to the API.")] bool includeGroupType = false, [OSParameter(Description = "When **true,** the response includes the signing group members.")] string includeUsers = "", [OSParameter(Description = "Set to true to send the include_users query parameter to the API.")] bool includeIncludeUsers = false);

    [OSAction(Description = "Updates signing group names.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    SigningGroupInformation SigningGroupsPutSigningGroups([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] SigningGroupInformation requestBody);

    [OSAction(Description = "Creates a signing group.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    SigningGroupInformation SigningGroupsPostSigningGroups([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] SigningGroupInformation requestBody);

    [OSAction(Description = "Deletes one or more signing groups.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    SigningGroupInformation SigningGroupsDeleteSigningGroups([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] SigningGroupInformation requestBody);

    [OSAction(Description = "Gets information about a signing group.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    SigningGroup SigningGroupsGetSigningGroup([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the [signing group](https://support.docusign.com/s/document-item?bundleId=gav1643676262430&topicId=zgn1578456447934.html).")] string signingGroupId);

    [OSAction(Description = "Updates a signing group.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    SigningGroup SigningGroupsPutSigningGroup([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the [signing group](https://support.docusign.com/s/document-item?bundleId=gav1643676262430&topicId=zgn1578456447934.html).")] string signingGroupId, [OSParameter(Description = "The JSON request body payload.")] SigningGroup requestBody);

    [OSAction(Description = "Gets a list of members in a Signing Group.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    SigningGroupUsers SigningGroupsGetSigningGroupUsers([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the [signing group](https://support.docusign.com/s/document-item?bundleId=gav1643676262430&topicId=zgn1578456447934.html). **Note:** When you send an envelope to a signing group, anyone in the group can open it and sign it with their own signature. For this reason, Docusign recommends that you do not include non-signer recipients (such as carbon copy recipients) in the same signing group as signer recipients. However, you could create a second signing group for the non-signer recipients and change t he default action of Needs to Sign to a different value, such as Receives a Copy.")] string signingGroupId);

    [OSAction(Description = "Adds members to a signing group.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    SigningGroupUsers SigningGroupsPutSigningGroupUsers([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the [signing group](https://support.docusign.com/s/document-item?bundleId=gav1643676262430&topicId=zgn1578456447934.html). **Note:** When you send an envelope to a signing group, anyone in the group can open it and sign it with their own signature. For this reason, Docusign recommends that you do not include non-signer recipients (such as carbon copy recipients) in the same signing group as signer recipients. However, you could create a second signing group for the non-signer recipients and change t he default action of Needs to Sign to a different value, such as Receives a Copy.")] string signingGroupId, [OSParameter(Description = "The JSON request body payload.")] SigningGroupUsers requestBody);

    [OSAction(Description = "Deletes  one or more members from a signing group.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    SigningGroupUsers SigningGroupsDeleteSigningGroupUsers([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the [signing group](https://support.docusign.com/s/document-item?bundleId=gav1643676262430&topicId=zgn1578456447934.html). **Note:** When you send an envelope to a signing group, anyone in the group can open it and sign it with their own signature. For this reason, Docusign recommends that you do not include non-signer recipients (such as carbon copy recipients) in the same signing group as signer recipients. However, you could create a second signing group for the non-signer recipients and change t he default action of Needs to Sign to a different value, such as Receives a Copy.")] string signingGroupId, [OSParameter(Description = "The JSON request body payload.")] SigningGroupUsers requestBody);

    [OSAction(Description = "Gets the supported languages for envelope recipients.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    SupportedLanguages SupportedLanguagesGetSupportedLanguages([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Gets a list of all account tabs.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    TabMetadataList TabsGetTabDefinitions([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "When **true,** only custom tabs are returned in the response.")] string customTabOnly = "", [OSParameter(Description = "Set to true to send the custom_tab_only query parameter to the API.")] bool includeCustomTabOnly = false);

    [OSAction(Description = "Creates a custom tab.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    TabMetadata TabsPostTabDefinitions([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] TabMetadata requestBody);

    [OSAction(Description = "Gets custom tab information.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    TabMetadata TabGetCustomTab([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The Docusign-generated custom tab ID for the custom tab to be applied. This can only be used when adding new tabs for a recipient. When used, the new tab inherits all the custom tab properties.")] string customTabId);

    [OSAction(Description = "Updates custom tab information.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    TabMetadata TabPutCustomTab([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The Docusign-generated custom tab ID for the custom tab to be applied. This can only be used when adding new tabs for a recipient. When used, the new tab inherits all the custom tab properties.")] string customTabId, [OSParameter(Description = "The JSON request body payload.")] TabMetadata requestBody);

    [OSAction(Description = "Deletes custom tab information.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] TabDeleteCustomTab([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The Docusign-generated custom tab ID for the custom tab to be applied. This can only be used when adding new tabs for a recipient. When used, the new tab inherits all the custom tab properties.")] string customTabId);

    [OSAction(Description = "Gets the list of templates.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeTemplateResults TemplatesGetTemplates([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The maximum number of results to return. Use `start_position` to specify the number of results to skip. **Note:** If the `count` parameter is not used, `listTemplates` has a default limit of 2,000 templates. If the account has more than 2,000 templates, `listTemplates` will return the first 2,000 templates. To retrieve more than 2,000 templates, repeat the API call, specifying `start_position` and `count` to control the number of templates retrieved.")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "Lists templates created on or after this date.")] string createdFromDate = "", [OSParameter(Description = "Set to true to send the created_from_date query parameter to the API.")] bool includeCreatedFromDate = false, [OSParameter(Description = "Lists templates modified before this date.")] string createdToDate = "", [OSParameter(Description = "Set to true to send the created_to_date query parameter to the API.")] bool includeCreatedToDate = false, [OSParameter(Description = "A comma-separated list of folder ID GUIDs.")] string folderIds = "", [OSParameter(Description = "Set to true to send the folder_ids query parameter to the API.")] bool includeFolderIds = false, [OSParameter(Description = "The type of folder to return templates for. Possible values are: - `templates`: Templates in the **My Templates** folder. Templates in the **Shared Templates**  and **All Template** folders (if the request ID from and Admin) are excluded. - `templates_root`: Templates in the root level of the **My Templates** folder, but not in an actual folder. Note that the **My Templates** folder is not a real folder. - `recylebin`: Templates that have been deleted.")] string folderTypes = "", [OSParameter(Description = "Set to true to send the folder_types query parameter to the API.")] bool includeFolderTypes = false, [OSParameter(Description = "Start of the search date range. Only returns templates created on or after this date/time. If no value is specified, there is no limit on the earliest date created.")] string fromDate = "", [OSParameter(Description = "Set to true to send the from_date query parameter to the API.")] bool includeFromDate = false, [OSParameter(Description = "A comma-separated list of additional template attributes to include in the response. Valid values are: - `powerforms`: Includes details about the PowerForms associated with the templates. - `documents`: Includes information about template documents. - `folders`: Includes information about the folder that holds the template. - `favorite_template_status`: Includes the template `favoritedByMe` property. **Note:** You can mark a template as a favorite only in eSignature v2.1. - `advanced_templates`: Includes information about advanced templates. - `recipients`: Includes information about template recipients. - `custom_fields`: Includes information about template custom fields. - `notifications`: Includes information about the notification settings for templates.")] string include = "", [OSParameter(Description = "Set to true to send the include query parameter to the API.")] bool includeInclude = false, [OSParameter(Description = "When **true,** retrieves templates that have been permanently deleted. The default is **false.** **Note:** After you delete a template, you can see it in the `Deleted` bin in the UI for 24 hours. After 24 hours, the template is permanently deleted.")] string isDeletedTemplateOnly = "", [OSParameter(Description = "Set to true to send the is_deleted_template_only query parameter to the API.")] bool includeIsDeletedTemplateOnly = false, [OSParameter(Description = "When **true,** downloads the templates listed in `template_ids` as a collection of JSON definitions in a single zip file. The `Content-Disposition` header is set in the response. The value of the header provides the filename of the file. The default is **false.** **Note:** This parameter only works when you specify a list of templates in the `template_ids` parameter.")] string isDownload = "", [OSParameter(Description = "Set to true to send the is_download query parameter to the API.")] bool includeIsDownload = false, [OSParameter(Description = "Lists templates modified on or after this date.")] string modifiedFromDate = "", [OSParameter(Description = "Set to true to send the modified_from_date query parameter to the API.")] bool includeModifiedFromDate = false, [OSParameter(Description = "Lists templates modified before this date.")] string modifiedToDate = "", [OSParameter(Description = "Set to true to send the modified_to_date query parameter to the API.")] bool includeModifiedToDate = false, [OSParameter(Description = "Specifies the sort order of the search results. Valid values are: - `asc`: Ascending (A to Z) - `desc`: Descending (Z to A)")] string order = "", [OSParameter(Description = "Set to true to send the order query parameter to the API.")] bool includeOrder = false, [OSParameter(Description = "Specifies how the search results are listed. Valid values are: - `name`: template name - `modified`: date/time template was last modified - `used`: date/time the template was last used.")] string orderBy = "", [OSParameter(Description = "Set to true to send the order_by query parameter to the API.")] bool includeOrderBy = false, [OSParameter(Description = "A comma-separated list of additional template properties to search. - `sender`: Include sender name and email in the search. - `recipients`: Include recipient names and emails in the search. - `envelope`: Not used in template searches.")] string searchFields = "", [OSParameter(Description = "Set to true to send the search_fields query parameter to the API.")] bool includeSearchFields = false, [OSParameter(Description = "The text to use to search the names of templates. Limit: 48 characters.")] string searchText = "", [OSParameter(Description = "Set to true to send the search_text query parameter to the API.")] bool includeSearchText = false, [OSParameter(Description = "When **true,** the response only includes templates shared by the user. When **false,** the response only returns template not shared by the user. If not specified, templates are returned whether or not they have been shared by the user.")] string sharedByMe = "", [OSParameter(Description = "Set to true to send the shared_by_me query parameter to the API.")] bool includeSharedByMe = false, [OSParameter(Description = "The zero-based index of the result from which to start returning results. Use with `count` to limit the number of results. The default value is `0`.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false, [OSParameter(Description = "A comma-separated list of template IDs to download. This value is valid only when `is_download` is **true.**")] string templateIds = "", [OSParameter(Description = "Set to true to send the template_ids query parameter to the API.")] bool includeTemplateIds = false, [OSParameter(Description = "The end of a search date range in UTC DateTime format. When you use this parameter, only templates created up to this date and time are returned. **Note:** If this property is null, the value defaults to the current date.")] string toDate = "", [OSParameter(Description = "Set to true to send the to_date query parameter to the API.")] bool includeToDate = false, [OSParameter(Description = "Start of the search date range. Only returns templates used or edited on or after this date/time. If no value is specified, there is no limit on the earliest date used.")] string usedFromDate = "", [OSParameter(Description = "Set to true to send the used_from_date query parameter to the API.")] bool includeUsedFromDate = false, [OSParameter(Description = "End of the search date range. Only returns templates used or edited up to this date/time. If no value is provided, this defaults to the current date.")] string usedToDate = "", [OSParameter(Description = "Set to true to send the used_to_date query parameter to the API.")] bool includeUsedToDate = false, [OSParameter(Description = "Filters the templates in the response. Valid values are: - `owned_by_me`: Results include only templates owned by the user. - `shared_with_me`: Results include only templates shared with the user. - `all`:  Results include all templates owned or shared with the user.")] string userFilter = "", [OSParameter(Description = "Set to true to send the user_filter query parameter to the API.")] bool includeUserFilter = false, [OSParameter(Description = "The ID of the user.")] string userId = "", [OSParameter(Description = "Set to true to send the user_id query parameter to the API.")] bool includeUserId = false);

    [OSAction(Description = "", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    TemplateAutoMatchList TemplatesPutTemplates([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] TemplateAutoMatchList requestBody);

    [OSAction(Description = "Creates one or more templates.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    TemplateSummary TemplatesPostTemplates([OSParameter(Description = "(Required) The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeTemplate requestBody);

    [OSAction(Description = "Gets a specific template associated with a specified account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeTemplate TemplatesGetTemplate([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "A comma-separated list of additional template attributes to include in the response. Valid values are: - `powerforms`: Includes information about PowerForms. - `tabs`: Includes information about tabs. - `documents`: Includes information about documents. - `favorite_template_status`: : Includes the template `favoritedByMe` property in the response. **Note:** You can mark a template as a favorite only in eSignature v2.1.")] string include = "", [OSParameter(Description = "Set to true to send the include query parameter to the API.")] bool includeInclude = false);

    [OSAction(Description = "Updates an existing template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    TemplateUpdateSummary TemplatesPutTemplate([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeTemplate requestBody);

    [OSAction(Description = "Shares a template with a group.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    GroupInformation TemplatesPutTemplatePart([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "Currently, the only defined part is **groups.**")] string templatePart, [OSParameter(Description = "The JSON request body payload.")] GroupInformation requestBody);

    [OSAction(Description = "Removes a member group's sharing permissions for a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    GroupInformation TemplatesDeleteTemplatePart([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "Currently, the only defined part is **groups.**")] string templatePart, [OSParameter(Description = "The JSON request body payload.")] GroupInformation requestBody);

    [OSAction(Description = "Gets the custom document fields from a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    CustomFields CustomFieldsGetTemplateCustomFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId);

    [OSAction(Description = "Updates envelope custom fields in a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    CustomFields CustomFieldsPutTemplateCustomFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] TemplateCustomFields requestBody);

    [OSAction(Description = "Creates custom document fields in an existing template document.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    CustomFields CustomFieldsPostTemplateCustomFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] TemplateCustomFields requestBody);

    [OSAction(Description = "Deletes envelope custom fields in a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    CustomFields CustomFieldsDeleteTemplateCustomFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] TemplateCustomFields requestBody);

    [OSAction(Description = "Gets a list of documents associated with a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    TemplateDocumentsResult DocumentsGetTemplateDocuments([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The include_agreement_type query parameter.")] string includeAgreementType = "", [OSParameter(Description = "Set to true to send the include_agreement_type query parameter to the API.")] bool includeIncludeAgreementType = false, [OSParameter(Description = "Reserved for Docusign.")] string includeTabs = "", [OSParameter(Description = "Set to true to send the include_tabs query parameter to the API.")] bool includeIncludeTabs = false);

    [OSAction(Description = "Adds documents to a template document.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    TemplateDocumentsResult DocumentsPutTemplateDocuments([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeDefinition requestBody);

    [OSAction(Description = "Deletes documents from a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    TemplateDocumentsResult DocumentsDeleteTemplateDocuments([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeDefinition requestBody);

    [OSAction(Description = "Gets PDF documents from a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] DocumentsGetTemplateDocument([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "When **true,** the PDF bytes returned in the response are encrypted for all the key managers configured on your Docusign account. You can decrypt the documents by using the Key Manager DecryptDocument API method. For more information about Key Manager, see the Docusign Security Appliance Installation Guide that your organization received from Docusign.")] string encrypt = "", [OSParameter(Description = "Set to true to send the encrypt query parameter to the API.")] bool includeEncrypt = false, [OSParameter(Description = "The file_type query parameter.")] string fileType = "", [OSParameter(Description = "Set to true to send the file_type query parameter to the API.")] bool includeFileType = false, [OSParameter(Description = "When **true,** any document fields that a recipient changed are highlighted in yellow in the returned PDF document, and optional signatures or initials are outlined in red.")] string showChanges = "", [OSParameter(Description = "Set to true to send the show_changes query parameter to the API.")] bool includeShowChanges = false);

    [OSAction(Description = "Updates a template document.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeDocument DocumentsPutTemplateDocument([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] EnvelopeDefinition requestBody, [OSParameter(Description = "The is_envelope_definition query parameter.")] string isEnvelopeDefinition = "", [OSParameter(Description = "Set to true to send the is_envelope_definition query parameter to the API.")] bool includeIsEnvelopeDefinition = false);

    [OSAction(Description = "Gets the custom document fields for a an existing template document.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DocumentFieldsInformation DocumentFieldsGetTemplateDocumentFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The ID of the template.")] string templateId);

    [OSAction(Description = "Updates existing custom document fields in an existing template document.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DocumentFieldsInformation DocumentFieldsPutTemplateDocumentFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] DocumentFieldsInformation requestBody);

    [OSAction(Description = "Creates custom document fields in an existing template document.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DocumentFieldsInformation DocumentFieldsPostTemplateDocumentFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] DocumentFieldsInformation requestBody);

    [OSAction(Description = "Deletes custom document fields from an existing template document.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DocumentFieldsInformation DocumentFieldsDeleteTemplateDocumentFields([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] DocumentFieldsInformation requestBody);

    [OSAction(Description = "Gets the Original HTML Definition used to generate the Responsive HTML for a given document in a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DocumentHtmlDefinitionOriginals ResponsiveHtmlGetTemplateDocumentHtmlDefinitions([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The ID of the template.")] string templateId);

    [OSAction(Description = "Returns document page images based on input.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    PageImages PagesGetTemplatePageImages([OSParameter(Description = "(Required) The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "(Required) The ID of the document.")] string documentId, [OSParameter(Description = "(Required) The ID of the template.")] string templateId, [OSParameter(Description = "The maximum number of results to return.")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "The number of dots per inch (DPI) for the resulting images. Valid values are 1-310 DPI. The default value is 94.")] string dpi = "", [OSParameter(Description = "Set to true to send the dpi query parameter to the API.")] bool includeDpi = false, [OSParameter(Description = "Sets the maximum height of the returned images in pixels.")] string maxHeight = "", [OSParameter(Description = "Set to true to send the max_height query parameter to the API.")] bool includeMaxHeight = false, [OSParameter(Description = "Sets the maximum width of the returned images in pixels.")] string maxWidth = "", [OSParameter(Description = "Set to true to send the max_width query parameter to the API.")] bool includeMaxWidth = false, [OSParameter(Description = "When **true,** using cache is disabled and image information is retrieved from a database. **True** is the default value.")] string nocache = "", [OSParameter(Description = "Set to true to send the nocache query parameter to the API.")] bool includeNocache = false, [OSParameter(Description = "When **true,** changes display in the user interface.")] string showChanges = "", [OSParameter(Description = "Set to true to send the show_changes query parameter to the API.")] bool includeShowChanges = false, [OSParameter(Description = "The position within the total result set from which to start returning values. The value **thumbnail** may be used to return the page image.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false);

    [OSAction(Description = "Deletes a page from a document in an template.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] PagesDeleteTemplatePage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The page number being accessed.")] string pageNumber, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] PageRequest requestBody);

    [OSAction(Description = "Gets a page image from a template for display.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] PagesGetTemplatePageImage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The page number being accessed.")] string pageNumber, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The number of dots per inch (DPI) for the resulting images. Valid values are 1-310 DPI. The default value is 94.")] string dpi = "", [OSParameter(Description = "Set to true to send the dpi query parameter to the API.")] bool includeDpi = false, [OSParameter(Description = "Sets the maximum height of the returned images in pixels.")] string maxHeight = "", [OSParameter(Description = "Set to true to send the max_height query parameter to the API.")] bool includeMaxHeight = false, [OSParameter(Description = "Sets the maximum width of the returned images in pixels.")] string maxWidth = "", [OSParameter(Description = "Set to true to send the max_width query parameter to the API.")] bool includeMaxWidth = false, [OSParameter(Description = "The show_changes query parameter.")] string showChanges = "", [OSParameter(Description = "Set to true to send the show_changes query parameter to the API.")] bool includeShowChanges = false);

    [OSAction(Description = "Rotates page image from a template for display.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] PagesPutTemplatePageImage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The page number being accessed.")] string pageNumber, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] PageRequest requestBody);

    [OSAction(Description = "Returns tabs on the specified page.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    TemplateDocumentTabs TabsGetTemplatePageTabs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The page number being accessed.")] string pageNumber, [OSParameter(Description = "The ID of the template.")] string templateId);

    [OSAction(Description = "Creates a preview of the responsive version of a template document.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DocumentHtmlDefinitions ResponsiveHtmlPostTemplateDocumentRespons_7d05547f([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] DocumentHtmlDefinition requestBody);

    [OSAction(Description = "Returns tabs on a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    TemplateDocumentTabs TabsGetTemplateDocumentTabs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "Filters for tabs that occur on the pages that you specify. Enter as a comma-separated list of page Guids. Example: `page_numbers=2,6`")] string pageNumbers = "", [OSParameter(Description = "Set to true to send the page_numbers query parameter to the API.")] bool includePageNumbers = false);

    [OSAction(Description = "Updates the tabs for a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Tabs TabsPutTemplateDocumentTabs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] TemplateTabs requestBody);

    [OSAction(Description = "Adds tabs to a document in a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Tabs TabsPostTemplateDocumentTabs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] TemplateTabs requestBody);

    [OSAction(Description = "Deletes tabs from a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Tabs TabsDeleteTemplateDocumentTabs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The unique ID of the document within the envelope. Unlike other IDs in the eSignature API, you specify the `documentId` yourself. Typically the first document has the ID `1`, the second document `2`, and so on, but you can use any numbering scheme that fits within a 32-bit signed integer (1 through 2147483647). Tab objects have a `documentId` property that specifies the document on which to place the tab.")] string documentId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] TemplateTabs requestBody);

    [OSAction(Description = "Gets the Original HTML Definition used to generate the Responsive HTML for the template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DocumentHtmlDefinitionOriginals ResponsiveHtmlGetTemplateHtmlDefinitions([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId);

    [OSAction(Description = "Gets template lock information.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    LockInformation LockGetTemplateLock([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId);

    [OSAction(Description = "Updates a template lock.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    LockInformation LockPutTemplateLock([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] LockRequest requestBody);

    [OSAction(Description = "Locks a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    LockInformation LockPostTemplateLock([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] LockRequest requestBody);

    [OSAction(Description = "Deletes a template lock.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    LockInformation LockDeleteTemplateLock([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] LockRequest requestBody);

    [OSAction(Description = "Gets template notification information.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Notification NotificationGetTemplatesTemplateIdNotification([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId);

    [OSAction(Description = "Updates the notification  structure for an existing template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Notification NotificationPutTemplatesTemplateIdNotification([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] TemplateNotificationRequest requestBody);

    [OSAction(Description = "Gets recipient information from a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Recipients RecipientsGetTemplateRecipients([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "When **true** and `include_tabs` is set to **true,** all tabs with anchor tab properties are included in the response.")] string includeAnchorTabLocations = "", [OSParameter(Description = "Set to true to send the include_anchor_tab_locations query parameter to the API.")] bool includeIncludeAnchorTabLocations = false, [OSParameter(Description = "When **true,** the extended properties are included in the response.")] string includeExtended = "", [OSParameter(Description = "Set to true to send the include_extended query parameter to the API.")] bool includeIncludeExtended = false, [OSParameter(Description = "When **true,** the tab information associated with the recipient is included in the response.")] string includeTabs = "", [OSParameter(Description = "Set to true to send the include_tabs query parameter to the API.")] bool includeIncludeTabs = false);

    [OSAction(Description = "Updates recipients in a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    RecipientsUpdateSummary RecipientsPutTemplateRecipients([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] TemplateRecipients requestBody, [OSParameter(Description = "When **true,** resends the envelope to the recipients that you specify in the request body. Use this parameter to resend the envelope to a recipient who deleted the original email notification. **Note:** Correcting an envelope is a different process. Docusign always resends an envelope when you correct it, regardless of the value that you enter here.")] string resendEnvelope = "", [OSParameter(Description = "Set to true to send the resend_envelope query parameter to the API.")] bool includeResendEnvelope = false);

    [OSAction(Description = "Adds tabs for a recipient.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Recipients RecipientsPostTemplateRecipients([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] TemplateRecipients requestBody, [OSParameter(Description = "When **true,** resends the envelope to the recipients that you specify in the request body. Use this parameter to resend the envelope to a recipient who deleted the original email notification. **Note:** Correcting an envelope is a different process. Docusign always resends an envelope when you correct it, regardless of the value that you enter here.")] string resendEnvelope = "", [OSParameter(Description = "Set to true to send the resend_envelope query parameter to the API.")] bool includeResendEnvelope = false);

    [OSAction(Description = "Deletes recipients from a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Recipients RecipientsDeleteTemplateRecipients([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] TemplateRecipients requestBody);

    [OSAction(Description = "Deletes the specified recipient file from a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Recipients RecipientsDeleteTemplateRecipient([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] TemplateRecipients requestBody);

    [OSAction(Description = "Returns document visibility for a template recipient", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DocumentVisibilityList RecipientsGetTemplateRecipientDocumentVisibility([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId, [OSParameter(Description = "The ID of the template.")] string templateId);

    [OSAction(Description = "Updates document visibility for a template recipient", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    TemplateDocumentVisibilityList RecipientsPutTemplateRecipientDocumentVisibility([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] TemplateDocumentVisibilityList requestBody);

    [OSAction(Description = "Gets the tabs information for a signer or sign-in-person recipient in a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Tabs RecipientsGetTemplateRecipientTabs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "When **true,** all tabs with anchor tab properties are included in the response. The default value is **false.**")] string includeAnchorTabLocations = "", [OSParameter(Description = "Set to true to send the include_anchor_tab_locations query parameter to the API.")] bool includeIncludeAnchorTabLocations = false, [OSParameter(Description = "When **true,** the response includes metadata indicating which properties are editable.")] string includeMetadata = "", [OSParameter(Description = "Set to true to send the include_metadata query parameter to the API.")] bool includeIncludeMetadata = false);

    [OSAction(Description = "Updates the tabs for a recipient.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Tabs RecipientsPutTemplateRecipientTabs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] TemplateTabs requestBody);

    [OSAction(Description = "Adds tabs for a recipient.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Tabs RecipientsPostTemplateRecipientTabs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] TemplateTabs requestBody);

    [OSAction(Description = "Deletes the tabs associated with a recipient in a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Tabs RecipientsDeleteTemplateRecipientTabs([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "A local reference used to map recipients to other objects, such as specific document tabs. A `recipientId` must be either an integer or a GUID, and the `recipientId` must be unique within an envelope. For example, many envelopes assign the first recipient a `recipientId` of `1`.")] string recipientId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] TemplateTabs requestBody);

    [OSAction(Description = "Updates document visibility for template recipients", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    TemplateDocumentVisibilityList RecipientsPutTemplateRecipientsDocumentVisibility([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] TemplateDocumentVisibilityList requestBody);

    [OSAction(Description = "Creates a preview of the responsive versions of all of the documents associated with a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DocumentHtmlDefinitions ResponsiveHtmlPostTemplateResponsiveHtmlPreview([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] DocumentHtmlDefinition requestBody);

    [OSAction(Description = "Gets a URL for a template edit view.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ViewUrl ViewsPostTemplateEditView([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] TemplateViewRequest requestBody);

    [OSAction(Description = "Creates a template recipient preview.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ViewUrl ViewsPostTemplateRecipientPreview([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] RecipientPreviewRequest requestBody);

    [OSAction(Description = "Returns the workflow definition for a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Workflow TemplateWorkflowDefinitionGetTemplateWork_a5e5af69([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId);

    [OSAction(Description = "Updates the workflow definition for a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Workflow TemplateWorkflowDefinitionPutTemplateWork_f659df72([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] Workflow requestBody);

    [OSAction(Description = "Delete the workflow definition for a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] TemplateWorkflowDefinitionDeleteTemplateW_3824e45a([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId);

    [OSAction(Description = "Returns the scheduled sending rules for a template's workflow definition.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ScheduledSending TemplateWorkflowScheduledSendingGetTempla_7ca3441d([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId);

    [OSAction(Description = "Updates the scheduled sending rules for a template's workflow definition.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ScheduledSending TemplateWorkflowScheduledSendingPutTempla_3677d893([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] ScheduledSending requestBody);

    [OSAction(Description = "Deletes the scheduled sending rules for the template's workflow.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] TemplateWorkflowScheduledSendingDeleteTem_c0241f55([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId);

    [OSAction(Description = "Adds a new step to a template's workflow.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    WorkflowStep TemplateWorkflowStepPostTemplateWorkflowS_2e2b4e6f([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The JSON request body payload.")] WorkflowStep requestBody);

    [OSAction(Description = "Returns a specified workflow step for a specified envelope.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    WorkflowStep TemplateWorkflowStepGetTemplateWorkflowSt_2c37995b([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The ID of the workflow step.")] string workflowStepId);

    [OSAction(Description = "Updates a specified workflow step for a template.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    WorkflowStep TemplateWorkflowStepPutTemplateWorkflowSt_00ec792c([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The ID of the workflow step.")] string workflowStepId, [OSParameter(Description = "The JSON request body payload.")] WorkflowStep requestBody);

    [OSAction(Description = "Deletes a workflow step from an template's workflow definition.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] TemplateWorkflowStepDeleteTemplateWorkflo_21662918([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The ID of the workflow step.")] string workflowStepId);

    [OSAction(Description = "Returns the delayed routing rules for a template's workflow step definition.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DelayedRouting TemplateWorkflowDelayedRoutingGetTemplate_6bb9d0ab([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The ID of the workflow step.")] string workflowStepId);

    [OSAction(Description = "Updates the delayed routing rules for a template's workflow step.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DelayedRouting TemplateWorkflowDelayedRoutingPutTemplate_844b84c8([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The ID of the workflow step.")] string workflowStepId, [OSParameter(Description = "The JSON request body payload.")] DelayedRouting requestBody);

    [OSAction(Description = "Deletes the delayed routing rules for the specified template workflow step.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] TemplateWorkflowDelayedRoutingDeleteTempl_582fffd9([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the template.")] string templateId, [OSParameter(Description = "The ID of the workflow step.")] string workflowStepId);

    [OSAction(Description = "", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    TemplateAutoMatchList TemplatesAutoMatchPutTemplates([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] TemplateAutoMatchList requestBody);

    [OSAction(Description = "Gets a list of unsupported file types.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    FileTypeList UnsupportedFileTypesGetUnsupportedFileTypes([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Retrieves the list of users for the specified account. You can filter the users list to get specific users.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserInformationList UsersGetUsers([OSParameter(Description = "(Required) The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "When **true,** the custom settings information is returned for each user in the account. If this parameter is omitted, the default behavior is **false.**")] string additionalInfo = "", [OSParameter(Description = "Set to true to send the additional_info query parameter to the API.")] bool includeAdditionalInfo = false, [OSParameter(Description = "When **true,** returns only alternate administrators. These users are not administrators but will be set as such if all administrator memberships are closed. The default value is **false.**")] string alternateAdminsOnly = "", [OSParameter(Description = "Set to true to send the alternate_admins_only query parameter to the API.")] bool includeAlternateAdminsOnly = false, [OSParameter(Description = "The maximum number of results to return. Use `start_position` to specify the number of results to skip. Valid values: `1` to `100`")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "When **true,** return only users in [domains](https://support.docusign.com/s/document-item?rsc_301=&bundleId=rrf1583359212854&topicId=jub1589318086105.html) claimed by your organization. The default value is **false.**")] string domainUsersOnly = "", [OSParameter(Description = "Set to true to send the domain_users_only query parameter to the API.")] bool includeDomainUsersOnly = false, [OSParameter(Description = "Filters results based on the email address associated with the user that you want to return. **Note:** You can use either this parameter or the `email_substring` parameter, but not both. For older accounts, this parameter might return multiple users who are associated with a single email address.")] string email = "", [OSParameter(Description = "Set to true to send the email query parameter to the API.")] bool includeEmail = false, [OSParameter(Description = "Filters results based on a fragment of an email address. For example, you could enter `gmail.com` to return all users who have Gmail addresses. **Note:** You do not use a wildcard character with this parameter. You can use either this parameter or the `email` parameter, but not both.")] string emailSubstring = "", [OSParameter(Description = "Set to true to send the email_substring query parameter to the API.")] bool includeEmailSubstring = false, [OSParameter(Description = "Filters results based on one or more group IDs.")] string groupId = "", [OSParameter(Description = "Set to true to send the group_id query parameter to the API.")] bool includeGroupId = false, [OSParameter(Description = "The include_license query parameter.")] string includeLicense = "", [OSParameter(Description = "Set to true to send the include_license query parameter to the API.")] bool includeIncludeLicense = false, [OSParameter(Description = "When **true,** the response includes the `userSettings` object data in CSV format.")] string includeUsersettingsForCsv = "", [OSParameter(Description = "Set to true to send the include_usersettings_for_csv query parameter to the API.")] bool includeIncludeUsersettingsForCsv = false, [OSParameter(Description = "When **true,** the response includes the login status of each user.")] string loginStatus = "", [OSParameter(Description = "Set to true to send the login_status query parameter to the API.")] bool includeLoginStatus = false, [OSParameter(Description = "Return user records excluding the specified group IDs.")] string notGroupId = "", [OSParameter(Description = "Set to true to send the not_group_id query parameter to the API.")] bool includeNotGroupId = false, [OSParameter(Description = "The zero-based index of the result from which to start returning results. Use with `count` to limit the number of results. The default value is `0`.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false, [OSParameter(Description = "Filters results by user account status. A comma-separated list of any of the following: * `ActivationRequired` * `ActivationSent` * `Active` * `Closed` * `Disabled`")] string status = "", [OSParameter(Description = "Set to true to send the status query parameter to the API.")] bool includeStatus = false, [OSParameter(Description = "Filters the user records returned by the user name or a sub-string of user name.")] string userNameSubstring = "", [OSParameter(Description = "Set to true to send the user_name_substring query parameter to the API.")] bool includeUserNameSubstring = false);

    [OSAction(Description = "Changes one or more users in the specified account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserInformationList UsersPutUsers([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] UserInformationList requestBody, [OSParameter(Description = "The allow_all_languages query parameter.")] string allowAllLanguages = "", [OSParameter(Description = "Set to true to send the allow_all_languages query parameter to the API.")] bool includeAllowAllLanguages = false);

    [OSAction(Description = "Adds new users to the specified account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    NewUsersSummary UsersPostUsers([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] NewUsersDefinition requestBody);

    [OSAction(Description = "Closes one or more users in the account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UsersResponse UsersDeleteUsers([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] UserInfoList requestBody, [OSParameter(Description = "A list of groups to remove the user from. A comma-separated list of the following: - `Groups` - `PermissionSet` - `SigningGroupsEmail`")] string delete = "", [OSParameter(Description = "Set to true to send the delete query parameter to the API.")] bool includeDelete = false);

    [OSAction(Description = "Gets the user information for a specified user using a userId (GUID). To find a user based on their email address, use the list endpoint.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserInformation UserGetUser([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId, [OSParameter(Description = "Setting this parameter has no effect in this operation.")] string additionalInfo = "", [OSParameter(Description = "Set to true to send the additional_info query parameter to the API.")] bool includeAdditionalInfo = false, [OSParameter(Description = "Setting this parameter has no effect in this operation.")] string email = "", [OSParameter(Description = "Set to true to send the email query parameter to the API.")] bool includeEmail = false, [OSParameter(Description = "The include_license query parameter.")] string includeLicense = "", [OSParameter(Description = "Set to true to send the include_license query parameter to the API.")] bool includeIncludeLicense = false);

    [OSAction(Description = "Updates user information for the specified user.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserInformation UserPutUser([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId, [OSParameter(Description = "The JSON request body payload.")] UserInformation requestBody, [OSParameter(Description = "The allow_all_languages query parameter.")] string allowAllLanguages = "", [OSParameter(Description = "Set to true to send the allow_all_languages query parameter to the API.")] bool includeAllowAllLanguages = false);

    [OSAction(Description = "Creates a user authorization.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserAuthorization UserAuthorizationCreateUserAuthorization([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the principal user.")] string userId, [OSParameter(Description = "The JSON request body payload.")] UserAuthorizationCreateRequest requestBody);

    [OSAction(Description = "Returns the user authorization for a given authorization ID.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserAuthorization UserAuthorizationGetUserAuthorization([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user authorization.")] string authorizationId, [OSParameter(Description = "The ID of the principal user.")] string userId);

    [OSAction(Description = "Updates the start or end date for a user authorization.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserAuthorization UserAuthorizationUpdateUserAuthorization([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user authorization.")] string authorizationId, [OSParameter(Description = "The ID of the principal user.")] string userId, [OSParameter(Description = "The JSON request body payload.")] UserAuthorizationUpdateRequest requestBody);

    [OSAction(Description = "Deletes the user authorization.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] UserAuthorizationDeleteUserAuthorization([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user authorization.")] string authorizationId, [OSParameter(Description = "The ID of the principal user.")] string userId);

    [OSAction(Description = "Returns the authorizations for which the specified user is the principal user.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserAuthorizations UserAuthorizationsGetPrincipalUserAuthorizations([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the principal user.")] string userId, [OSParameter(Description = "When **true,** return only active authorizations. The default value is **true.**")] string activeOnly = "", [OSParameter(Description = "Set to true to send the active_only query parameter to the API.")] bool includeActiveOnly = false, [OSParameter(Description = "The maximum number of results to return.")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "Filters returned user records by full email address or a substring of email address.")] string emailSubstring = "", [OSParameter(Description = "Set to true to send the email_substring query parameter to the API.")] bool includeEmailSubstring = false, [OSParameter(Description = "When **true,** returns active and scheduled authorizations of closed users. The default value is **true.** This value is only applied when `active_only` is **false.**")] string includeClosedUsers = "", [OSParameter(Description = "Set to true to send the include_closed_users query parameter to the API.")] bool includeIncludeClosedUsers = false, [OSParameter(Description = "Filters results by authorization permission. Valid values: * `Send` * `Manage` * `Sign`")] string permissions = "", [OSParameter(Description = "Set to true to send the permissions query parameter to the API.")] bool includePermissions = false, [OSParameter(Description = "The position within the total result set from which to start returning values. The value **thumbnail** may be used to return the page image.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false, [OSParameter(Description = "Filters results based on a full or partial user name. **Note:** When you enter a partial user name, you do not use a wildcard character.")] string userNameSubstring = "", [OSParameter(Description = "Set to true to send the user_name_substring query parameter to the API.")] bool includeUserNameSubstring = false);

    [OSAction(Description = "Create or update multiple user authorizations.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserAuthorizationsResponse UserAuthorizationsPostUserAuthorizations([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the principal user.")] string userId, [OSParameter(Description = "The JSON request body payload.")] UserAuthorizationsRequest requestBody);

    [OSAction(Description = "Delete multiple user authorizations.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserAuthorizationsDeleteResponse UserAuthorizationsDeleteUserAuthorizations([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the principal user.")] string userId, [OSParameter(Description = "The JSON request body payload.")] UserAuthorizationsDeleteRequest requestBody);

    [OSAction(Description = "Returns the authorizations for which the specified user is the agent user.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserAuthorizations UserAgentAuthorizationsGetAgentUserAuthorizations([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The user who is acting as the agent.")] string userId, [OSParameter(Description = "When **true,** only active users are returned. The default value is **false.**")] string activeOnly = "", [OSParameter(Description = "Set to true to send the active_only query parameter to the API.")] bool includeActiveOnly = false, [OSParameter(Description = "The maximum number of results to return.")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "Filters returned user records by full email address or a substring of email address.")] string emailSubstring = "", [OSParameter(Description = "Set to true to send the email_substring query parameter to the API.")] bool includeEmailSubstring = false, [OSParameter(Description = "When **true,** returns active and scheduled authorizations of closed users. The default value is **true.** This value is only applied when `active_only` is **false.**")] string includeClosedUsers = "", [OSParameter(Description = "Set to true to send the include_closed_users query parameter to the API.")] bool includeIncludeClosedUsers = false, [OSParameter(Description = "The permissions query parameter.")] string permissions = "", [OSParameter(Description = "Set to true to send the permissions query parameter to the API.")] bool includePermissions = false, [OSParameter(Description = "The position within the total result set from which to start returning values. The value **thumbnail** may be used to return the page image.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false, [OSParameter(Description = "Filters results based on a full or partial user name. **Note:** When you enter a partial user name, you do not use a wildcard character.")] string userNameSubstring = "", [OSParameter(Description = "Set to true to send the user_name_substring query parameter to the API.")] bool includeUserNameSubstring = false);

    [OSAction(Description = "Get the Cloud Storage Provider configuration for the specified user.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    CloudStorageProviders CloudStorageGetCloudStorageProviders([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId, [OSParameter(Description = "The URL the user is redirected to after the cloud storage provider authenticates the user. Using this will append the redirectUrl to the authenticationUrl. The redirectUrl is restricted to URLs in the docusign.com or docusign.net domains.")] string redirectUrl = "", [OSParameter(Description = "Set to true to send the redirectUrl query parameter to the API.")] bool includeRedirectUrl = false);

    [OSAction(Description = "Configures the redirect URL information  for one or more cloud storage providers for the specified user.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    CloudStorageProviders CloudStoragePostCloudStorage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId, [OSParameter(Description = "The JSON request body payload.")] CloudStorageProviders requestBody);

    [OSAction(Description = "Deletes the user authentication information for one or more cloud storage providers.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    CloudStorageProviders CloudStorageDeleteCloudStorageProviders([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId, [OSParameter(Description = "The JSON request body payload.")] CloudStorageProviders requestBody);

    [OSAction(Description = "Gets the specified Cloud Storage Provider configuration for the User.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    CloudStorageProviders CloudStorageGetCloudStorage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the service to access. Valid values are the service name (\"Box\") or the numerical serviceId (\"4136\").")] string serviceId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId, [OSParameter(Description = "The URL the user is redirected to after the cloud storage provider authenticates the user. Using this will append the redirectUrl to the authenticationUrl. The redirectUrl is restricted to URLs in the docusign.com or docusign.net domains.")] string redirectUrl = "", [OSParameter(Description = "Set to true to send the redirectUrl query parameter to the API.")] bool includeRedirectUrl = false);

    [OSAction(Description = "Deletes the user authentication information for the specified cloud storage provider.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    CloudStorageProviders CloudStorageDeleteCloudStorage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the service to access. Valid values are the service name (\"Box\") or the numerical serviceId (\"4136\").")] string serviceId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId);

    [OSAction(Description = "Retrieves a list of all the items in a specified folder from the specified cloud storage provider.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ExternalFolder CloudStorageFolderGetCloudStorageFolderAll([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the service to access. Valid values are the service name (\"Box\") or the numerical serviceId (\"4136\").")] string serviceId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId, [OSParameter(Description = "A comma separated list of folder IDs included in the request.")] string cloudStorageFolderPath = "", [OSParameter(Description = "Set to true to send the cloud_storage_folder_path query parameter to the API.")] bool includeCloudStorageFolderPath = false, [OSParameter(Description = "The maximum number of results to return. Use `start_position` to specify the number of results to skip. Default: `25`")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "The order in which to sort the results. Valid values are: * `asc`: Ascending order. * `desc`: Descending order.")] string order = "", [OSParameter(Description = "Set to true to send the order query parameter to the API.")] bool includeOrder = false, [OSParameter(Description = "The file attribute to use to sort the results. Valid values are: * `modified` * `name`")] string orderBy = "", [OSParameter(Description = "Set to true to send the order_by query parameter to the API.")] bool includeOrderBy = false, [OSParameter(Description = "Use this parameter to search for specific text.")] string searchText = "", [OSParameter(Description = "Set to true to send the search_text query parameter to the API.")] bool includeSearchText = false, [OSParameter(Description = "The zero-based index of the result from which to start returning results. Use with `count` to limit the number of results. The default value is `0`.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false);

    [OSAction(Description = "Gets a list of items from a cloud storage provider.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ExternalFolder CloudStorageFolderGetCloudStorageFolder([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the folder.")] string folderId, [OSParameter(Description = "The ID of the service to access. Valid values are the service name (\"Box\") or the numerical serviceId (\"4136\").")] string serviceId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId, [OSParameter(Description = "The file path to a cloud storage folder.")] string cloudStorageFolderPath = "", [OSParameter(Description = "Set to true to send the cloud_storage_folder_path query parameter to the API.")] bool includeCloudStorageFolderPath = false, [OSParameter(Description = "A plain-text folder ID that you can use as an alternative to the existing folder id. This property is mainly used for rooms. Enter multiple folder IDs as a comma-separated list.")] string cloudStorageFolderidPlain = "", [OSParameter(Description = "Set to true to send the cloud_storage_folderid_plain query parameter to the API.")] bool includeCloudStorageFolderidPlain = false, [OSParameter(Description = "The maximum number of results to return. Use `start_position` to specify the number of results to skip. Default: `25`")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "The order in which to sort the results. Valid values are: * `asc`: Ascending order. * `desc`: Descending order.")] string order = "", [OSParameter(Description = "Set to true to send the order query parameter to the API.")] bool includeOrder = false, [OSParameter(Description = "The file attribute to use to sort the results. Valid values are: * `modified` * `name`")] string orderBy = "", [OSParameter(Description = "Set to true to send the order_by query parameter to the API.")] bool includeOrderBy = false, [OSParameter(Description = "Use this parameter to search for specific text.")] string searchText = "", [OSParameter(Description = "Set to true to send the search_text query parameter to the API.")] bool includeSearchText = false, [OSParameter(Description = "The sky_drive_skip_token query parameter.")] string skyDriveSkipToken = "", [OSParameter(Description = "Set to true to send the sky_drive_skip_token query parameter to the API.")] bool includeSkyDriveSkipToken = false, [OSParameter(Description = "The zero-based index of the result from which to start returning results. Use with `count` to limit the number of results. The default value is `0`.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false);

    [OSAction(Description = "Retrieves the custom user settings for a specified user.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    CustomSettingsInformation UserCustomSettingsGetCustomSettings([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId);

    [OSAction(Description = "Adds or updates custom user settings for the specified user.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    CustomSettingsInformation UserCustomSettingsPutCustomSettings([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId, [OSParameter(Description = "The JSON request body payload.")] CustomSettingsInformation requestBody);

    [OSAction(Description = "Deletes custom user settings for a specified user.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    CustomSettingsInformation UserCustomSettingsDeleteCustomSettings([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId, [OSParameter(Description = "The JSON request body payload.")] CustomSettingsInformation requestBody);

    [OSAction(Description = "Retrieves the user profile for a specified user.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserProfile UserProfileGetProfile([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId);

    [OSAction(Description = "Updates the user profile information for the specified user.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] UserProfilePutProfile([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId, [OSParameter(Description = "The JSON request body payload.")] UserProfile requestBody);

    [OSAction(Description = "Retrieves the user profile image for the specified user.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] UserProfileImageGetUserProfileImage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId, [OSParameter(Description = "Reserved for Docusign.")] string encoding = "", [OSParameter(Description = "Set to true to send the encoding query parameter to the API.")] bool includeEncoding = false);

    [OSAction(Description = "Updates the user profile image for a specified user.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] UserProfileImagePutUserProfileImage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId);

    [OSAction(Description = "Deletes the user profile image for the specified user.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] UserProfileImageDeleteUserProfileImage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId);

    [OSAction(Description = "Gets the user account settings for a specified user.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserSettingsInformation UserSettingsGetUserSettings([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId);

    [OSAction(Description = "Updates the user account settings for a specified user.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] UserSettingsPutUserSettings([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId, [OSParameter(Description = "The JSON request body payload.")] UserSettingsInformation requestBody, [OSParameter(Description = "The allow_all_languages query parameter.")] string allowAllLanguages = "", [OSParameter(Description = "Set to true to send the allow_all_languages query parameter to the API.")] bool includeAllowAllLanguages = false);

    [OSAction(Description = "Retrieves a list of signature definitions for a user.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserSignaturesInformation UserSignaturesGetUserSignatures([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId, [OSParameter(Description = "The type of stamps to return. Valid values are: - `signature`: Returns information about signature images only. This is the default value. - `stamp`: Returns information about eHanko and custom stamps only. - null")] string stampType = "", [OSParameter(Description = "Set to true to send the stamp_type query parameter to the API.")] bool includeStampType = false);

    [OSAction(Description = "Adds/updates a user signature.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserSignaturesInformation UserSignaturesPutUserSignature([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId, [OSParameter(Description = "The JSON request body payload.")] UserSignaturesInformation requestBody);

    [OSAction(Description = "Adds user Signature and initials images to a Signature.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserSignaturesInformation UserSignaturesPostUserSignatures([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId, [OSParameter(Description = "The JSON request body payload.")] UserSignaturesInformation requestBody);

    [OSAction(Description = "Gets the user signature information for the specified user.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserSignature UserSignaturesGetUserSignature([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the account stamp.")] string signatureId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId);

    [OSAction(Description = "Updates the user signature for a specified user.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserSignature UserSignaturesPutUserSignatureById([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the account stamp.")] string signatureId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId, [OSParameter(Description = "The JSON request body payload.")] UserSignatureDefinition requestBody, [OSParameter(Description = "When **true,** closes the current signature.")] string closeExistingSignature = "", [OSParameter(Description = "Set to true to send the close_existing_signature query parameter to the API.")] bool includeCloseExistingSignature = false);

    [OSAction(Description = "Removes removes signature information for the specified user.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] UserSignaturesDeleteUserSignature([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the account stamp.")] string signatureId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId);

    [OSAction(Description = "Retrieves the user initials image or the  user signature image for the specified user.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] UserSignaturesGetUserSignatureImage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "Specificies the type of image. Valid values: - `stamp_image` - `signature_image` - `initials_image`")] string imageType, [OSParameter(Description = "The ID of the account stamp.")] string signatureId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId, [OSParameter(Description = "When **true,** the chrome (or frame containing the added line and identifier) is included with the signature image.")] string includeChrome = "", [OSParameter(Description = "Set to true to send the include_chrome query parameter to the API.")] bool includeIncludeChrome = false);

    [OSAction(Description = "Updates the user signature image or user initials image for the specified user.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserSignature UserSignaturesPutUserSignatureImage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "Specificies the type of image. Valid values: - `stamp_image` - `signature_image` - `initials_image`")] string imageType, [OSParameter(Description = "The ID of the account stamp.")] string signatureId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId, [OSParameter(Description = "The binary request body payload.")] byte[] requestBody, [OSParameter(Description = "The transparent_png query parameter.")] string transparentPng = "", [OSParameter(Description = "Set to true to send the transparent_png query parameter to the API.")] bool includeTransparentPng = false);

    [OSAction(Description = "Deletes the user initials image or the  user signature image for the specified user.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserSignature UserSignaturesDeleteUserSignatureImage([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "Specificies the type of image. Valid values: - `stamp_image` - `signature_image` - `initials_image`")] string imageType, [OSParameter(Description = "The ID of the account stamp.")] string signatureId, [OSParameter(Description = "The ID of the user to access. **Note:** Users can only access their own information. A user, even one with Admin rights, cannot access another user's settings.")] string userId);

    [OSAction(Description = "Returns a URL to the Docusign eSignature web application.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    EnvelopeViews ViewsPostAccountConsoleView([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] ConsoleViewRequest requestBody);

    [OSAction(Description = "Get watermark information.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Watermark WatermarkGetWatermark([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Update watermark information.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Watermark WatermarkPutWatermark([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] Watermark requestBody);

    [OSAction(Description = "Get watermark preview.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Watermark WatermarkPreviewPutWatermarkPreview([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] Watermark requestBody);

    [OSAction(Description = "List Workspaces", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    WorkspaceList WorkspaceGetWorkspaces([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId);

    [OSAction(Description = "Create a Workspace", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Workspace WorkspacePostWorkspace([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The JSON request body payload.")] Workspace requestBody);

    [OSAction(Description = "Get Workspace", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Workspace WorkspaceGetWorkspace([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the workspace.")] string workspaceId);

    [OSAction(Description = "Update Workspace", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Workspace WorkspacePutWorkspace([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the workspace.")] string workspaceId, [OSParameter(Description = "The JSON request body payload.")] Workspace requestBody);

    [OSAction(Description = "Delete Workspace", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Workspace WorkspaceDeleteWorkspace([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the workspace.")] string workspaceId);

    [OSAction(Description = "List workspace folder contents", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    WorkspaceFolderContents WorkspaceFolderGetWorkspaceFolder([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the folder.")] string folderId, [OSParameter(Description = "The ID of the workspace.")] string workspaceId, [OSParameter(Description = "The maximum number of results to return. Use `start_position` to specify the number of results to skip.")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "When **true,** the response includes file information (in addition to folder information). The default is **false.**")] string includeFiles = "", [OSParameter(Description = "Set to true to send the include_files query parameter to the API.")] bool includeIncludeFiles = false, [OSParameter(Description = "When **true,** the response includes information about the sub-folders of the current folder. The default is **false.**")] string includeSubFolders = "", [OSParameter(Description = "Set to true to send the include_sub_folders query parameter to the API.")] bool includeIncludeSubFolders = false, [OSParameter(Description = "When **true,** the response returns thumbnails.  The default is **false.**")] string includeThumbnails = "", [OSParameter(Description = "Set to true to send the include_thumbnails query parameter to the API.")] bool includeIncludeThumbnails = false, [OSParameter(Description = "When **true,** the response includes extended details about the user. The default is **false.**")] string includeUserDetail = "", [OSParameter(Description = "Set to true to send the include_user_detail query parameter to the API.")] bool includeIncludeUserDetail = false, [OSParameter(Description = "The zero-based index of the result from which to start returning results. Use with `count` to limit the number of results. The default value is `0`.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false, [OSParameter(Description = "If set, the response only includes results associated with the `userId` that you specify.")] string workspaceUserId = "", [OSParameter(Description = "Set to true to send the workspace_user_id query parameter to the API.")] bool includeWorkspaceUserId = false);

    [OSAction(Description = "Deletes files or sub-folders from a workspace.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] WorkspaceFolderDeleteWorkspaceItems([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the folder.")] string folderId, [OSParameter(Description = "The ID of the workspace.")] string workspaceId, [OSParameter(Description = "The JSON request body payload.")] WorkspaceItemList requestBody);

    [OSAction(Description = "Creates a workspace file.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    WorkspaceItem WorkspaceFilePostWorkspaceFiles([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the folder.")] string folderId, [OSParameter(Description = "The ID of the workspace.")] string workspaceId);

    [OSAction(Description = "Gets a workspace file", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] WorkspaceFileGetWorkspaceFile([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the file.")] string fileId, [OSParameter(Description = "The ID of the folder.")] string folderId, [OSParameter(Description = "The ID of the workspace.")] string workspaceId, [OSParameter(Description = "When **true,** the `Content-Disposition` header is set in the response. The value of the header provides the filename of the file. The default is **false.**")] string isDownload = "", [OSParameter(Description = "Set to true to send the is_download query parameter to the API.")] bool includeIsDownload = false, [OSParameter(Description = "When **true** the file is returned in PDF format.")] string pdfVersion = "", [OSParameter(Description = "Set to true to send the pdf_version query parameter to the API.")] bool includePdfVersion = false);

    [OSAction(Description = "Update workspace file or folder metadata", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    WorkspaceItem WorkspaceFilePutWorkspaceFile([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the file.")] string fileId, [OSParameter(Description = "The ID of the folder.")] string folderId, [OSParameter(Description = "The ID of the workspace.")] string workspaceId);

    [OSAction(Description = "List File Pages", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    PageImages WorkspaceFilePagesGetWorkspaceFilePages([OSParameter(Description = "The external account number (int) or account ID GUID.")] string accountId, [OSParameter(Description = "The ID of the file.")] string fileId, [OSParameter(Description = "The ID of the folder.")] string folderId, [OSParameter(Description = "The ID of the workspace.")] string workspaceId, [OSParameter(Description = "The maximum number of results to return. Use `start_position` to specify the number of results to skip.")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "The number of dots per inch (DPI) for the resulting images. Valid values are 1-310 DPI. The default value is 94.")] string dpi = "", [OSParameter(Description = "Set to true to send the dpi query parameter to the API.")] bool includeDpi = false, [OSParameter(Description = "Sets the maximum height of the returned images in pixels.")] string maxHeight = "", [OSParameter(Description = "Set to true to send the max_height query parameter to the API.")] bool includeMaxHeight = false, [OSParameter(Description = "Sets the maximum width of the returned images in pixels.")] string maxWidth = "", [OSParameter(Description = "Set to true to send the max_width query parameter to the API.")] bool includeMaxWidth = false, [OSParameter(Description = "The zero-based index of the result from which to start returning results. Use with `count` to limit the number of results. The default value is `0`.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false);

    [OSAction(Description = "Retrieves the account provisioning information for the account.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ProvisioningInformation AccountsGetProvisioning();

    [OSAction(Description = "Gets a list of available billing plans.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BillingPlansResponse BillingPlansGetBillingPlans();

    [OSAction(Description = "Gets billing plan details.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    BillingPlanResponse BillingPlansGetBillingPlan([OSParameter(Description = "The ID of the billing plan being accessed.")] string billingPlanId);

    [OSAction(Description = "Gets settings for a  notary user.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    NotaryResult NotaryGetNotary([OSParameter(Description = "When **true,** the response will include a `jurisdiction` property that contains an array of all supported jurisdictions for the current user.")] string includeJurisdictions = "", [OSParameter(Description = "Set to true to send the include_jurisdictions query parameter to the API.")] bool includeIncludeJurisdictions = false);

    [OSAction(Description = "Updates notary information for the current user.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Notary NotaryPutNotary([OSParameter(Description = "The JSON request body payload.")] Notary requestBody);

    [OSAction(Description = "Registers the current user as a notary.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    Notary NotaryPostNotary([OSParameter(Description = "The JSON request body payload.")] Notary requestBody);

    [OSAction(Description = "Gets notary jurisdictions for a user.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    NotaryJournalList NotaryJournalsGetNotaryJournals([OSParameter(Description = "The maximum number of results to return.")] string count = "", [OSParameter(Description = "Set to true to send the count query parameter to the API.")] bool includeCount = false, [OSParameter(Description = "Use this parameter to search for specific text.")] string searchText = "", [OSParameter(Description = "Set to true to send the search_text query parameter to the API.")] bool includeSearchText = false, [OSParameter(Description = "The position within the total result set from which to start returning values. The value **thumbnail** may be used to return the page image.")] string startPosition = "", [OSParameter(Description = "Set to true to send the start_position query parameter to the API.")] bool includeStartPosition = false);

    [OSAction(Description = "Returns a list of jurisdictions that the notary is registered in.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    NotaryJurisdictionList NotaryJurisdictionsGetNotaryJurisdictions();

    [OSAction(Description = "Creates a jurisdiction object.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    NotaryJurisdiction NotaryJurisdictionsPostNotaryJurisdictions([OSParameter(Description = "The JSON request body payload.")] NotaryJurisdiction requestBody);

    [OSAction(Description = "Gets a jurisdiction object for the current user. The user must be a notary.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    NotaryJurisdiction NotaryJurisdictionsGetNotaryJurisdiction([OSParameter(Description = "The ID of the jurisdiction. The following jurisdictions are supported: -  `5 - California` -  `6 - Colorado` -  `9 - Florida` -  `10 - Georgia` -  `12 - Idaho` -  `13 - Illinois` -  `14 - Indiana` -  `15 - Iowa` -  `17 - Kentucky` -  `23 - Minnesota` -  `25 - Missouri` -  `30 - New Jersey` -  `32 - New York` -  `33 - North Carolina` -  `35 - Ohio` -  `37 - Oregon` -  `38 - Pennsylvania` -  `40 - South Carolina` -  `43 - Texas` -  `44 - Utah` -  `47 - Washington` -  `48 - West Virginia` -  `49 - Wisconsin` -  `62 - Florida Commissioner of Deeds`")] string jurisdictionId);

    [OSAction(Description = "Updates the jurisdiction information about a notary.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    NotaryJurisdiction NotaryJurisdictionsPutNotaryJurisdiction([OSParameter(Description = "The ID of the jurisdiction. The following jurisdictions are supported: -  `5 - California` -  `6 - Colorado` -  `9 - Florida` -  `10 - Georgia` -  `12 - Idaho` -  `13 - Illinois` -  `14 - Indiana` -  `15 - Iowa` -  `17 - Kentucky` -  `23 - Minnesota` -  `25 - Missouri` -  `30 - New Jersey` -  `32 - New York` -  `33 - North Carolina` -  `35 - Ohio` -  `37 - Oregon` -  `38 - Pennsylvania` -  `40 - South Carolina` -  `43 - Texas` -  `44 - Utah` -  `47 - Washington` -  `48 - West Virginia` -  `49 - Wisconsin` -  `62 - Florida Commissioner of Deeds`")] string jurisdictionId, [OSParameter(Description = "The JSON request body payload.")] NotaryJurisdiction requestBody);

    [OSAction(Description = "Deletes the specified jurisdiction.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] NotaryJurisdictionsDeleteNotaryJurisdiction([OSParameter(Description = "The ID of the jurisdiction. The following jurisdictions are supported: -  `5 - California` -  `6 - Colorado` -  `9 - Florida` -  `10 - Georgia` -  `12 - Idaho` -  `13 - Illinois` -  `14 - Indiana` -  `15 - Iowa` -  `17 - Kentucky` -  `23 - Minnesota` -  `25 - Missouri` -  `30 - New Jersey` -  `32 - New York` -  `33 - North Carolina` -  `35 - Ohio` -  `37 - Oregon` -  `38 - Pennsylvania` -  `40 - South Carolina` -  `43 - Texas` -  `44 - Utah` -  `47 - Washington` -  `48 - West Virginia` -  `49 - Wisconsin` -  `62 - Florida Commissioner of Deeds`")] string jurisdictionId);

    [OSAction(Description = "Gets membership account password rules.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    UserPasswordRules PasswordRulesGetPasswordRules();

    [OSAction(Description = "Gets the API request logging log files.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    ApiRequestLogsResult APIRequestLogGetRequestLogs([OSParameter(Description = "Reserved for Docusign.")] string encoding = "", [OSParameter(Description = "Set to true to send the encoding query parameter to the API.")] bool includeEncoding = false);

    [OSAction(Description = "Deletes the request log files.", ReturnDescription = "The result returned by the API.", ReturnName = "Data")]
    byte[] APIRequestLogDeleteRequestLogs();

    [OSAction(Description = "Gets a request logging log file.", ReturnDescription = "The result returned by the API.", ReturnName = "Text")]
    string APIRequestLogGetRequestLog([OSParameter(Description = "The ID of the log entry.")] string requestLogId);

    [OSAction(Description = "Gets the API request logging settings.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DiagnosticsSettingsInformation APIRequestLogGetRequestLogSettings();

    [OSAction(Description = "Enables or disables API request logging for troubleshooting.", ReturnDescription = "The result returned by the API.", ReturnName = "Item")]
    DiagnosticsSettingsInformation APIRequestLogPutRequestLogSettings([OSParameter(Description = "The JSON request body payload.")] DiagnosticsSettingsInformation requestBody);

}
