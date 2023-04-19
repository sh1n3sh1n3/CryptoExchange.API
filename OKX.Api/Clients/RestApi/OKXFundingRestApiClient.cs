﻿namespace OKX.Api.Clients.RestApi;

public class OKXFundingRestApiClient : OKXBaseRestApiClient
{
    // Endpoints
    protected const string Endpoints_V5_Asset_Currencies = "api/v5/asset/currencies";
    protected const string Endpoints_V5_Asset_Balances = "api/v5/asset/balances";
    protected const string Endpoints_V5_Asset_Transfer = "api/v5/asset/transfer";
    protected const string Endpoints_V5_Asset_Bills = "api/v5/asset/bills";
    protected const string Endpoints_V5_Asset_DepositLightning = "api/v5/asset/deposit-lightning";
    protected const string Endpoints_V5_Asset_DepositAddress = "api/v5/asset/deposit-address";
    protected const string Endpoints_V5_Asset_DepositHistory = "api/v5/asset/deposit-history";
    protected const string Endpoints_V5_Asset_Withdrawal = "api/v5/asset/withdrawal";
    protected const string Endpoints_V5_Asset_WithdrawalLightning = "api/v5/asset/withdrawal-lightning";
    protected const string Endpoints_V5_Asset_WithdrawalCancel = "api/v5/asset/cancel-withdrawal";
    protected const string Endpoints_V5_Asset_WithdrawalHistory = "api/v5/asset/withdrawal-history";
    protected const string Endpoints_V5_Asset_SavingBalance = "api/v5/asset/saving-balance";
    protected const string Endpoints_V5_Asset_SavingPurchaseRedempt = "api/v5/asset/purchase_redempt";

    internal OKXFundingRestApiClient(OKXRestApiClient root) : base(root)
    {
    }

    #region Funding API Endpoints
    /// <summary>
    /// Retrieve a list of all currencies. Not all currencies can be traded. Currencies that have not been defined in ISO 4217 may use a custom symbol.
    /// </summary>
    /// <param name="ct">Cancellation Token</param>
    /// <returns></returns>
    public virtual async Task<RestCallResult<IEnumerable<OkxCurrency>>> GetCurrenciesAsync(CancellationToken ct = default)
    {
        return await SendOKXRequest<IEnumerable<OkxCurrency>>(RootClient.GetUri(Endpoints_V5_Asset_Currencies), HttpMethod.Get, ct, true).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieve the balances of all the assets, and the amount that is available or on hold.
    /// </summary>
    /// <param name="currency">Currency</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns></returns>
    public virtual async Task<RestCallResult<IEnumerable<OkxFundingBalance>>> GetFundingBalanceAsync(string currency = null, CancellationToken ct = default)
    {
        var parameters = new Dictionary<string, object>();
        parameters.AddOptionalParameter("ccy", currency);

        return await SendOKXRequest<IEnumerable<OkxFundingBalance>>(RootClient.GetUri(Endpoints_V5_Asset_Balances), HttpMethod.Get, ct, signed: true, queryParameters: parameters).ConfigureAwait(false);
    }

    // TODO: Get account asset valuation

    /// <summary>
    /// This endpoint supports the transfer of funds between your funding account and trading account, and from the master account to sub-accounts. Direct transfers between sub-accounts are not allowed.
    /// </summary>
    /// <param name="currency">Currency</param>
    /// <param name="amount">Amount</param>
    /// <param name="type">Transfer type</param>
    /// <param name="fromAccount">The remitting account</param>
    /// <param name="toAccount">The beneficiary account</param>
    /// <param name="subAccountName">Sub Account Name</param>
    /// <param name="fromInstrumentId">MARGIN trading pair (e.g. BTC-USDT) or contract underlying (e.g. BTC-USD) to be transferred out.</param>
    /// <param name="toInstrumentId">MARGIN trading pair (e.g. BTC-USDT) or contract underlying (e.g. BTC-USD) to be transferred in.</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns></returns>
    public virtual async Task<RestCallResult<OkxTransferResponse>> FundTransferAsync(
        string currency,
        decimal amount,
        OkxTransferType type,
        OkxAccount fromAccount,
        OkxAccount toAccount,
        string subAccountName = null,
        string fromInstrumentId = null,
        string toInstrumentId = null,
        CancellationToken ct = default)
    {
        var parameters = new Dictionary<string, object> {
            { "ccy",currency},
            { "amt",amount.ToString(OkxGlobals.OkxCultureInfo)},
            { "type", JsonConvert.SerializeObject(type, new TransferTypeConverter(false)) },
            { "from", JsonConvert.SerializeObject(fromAccount, new AccountConverter(false)) },
            { "to", JsonConvert.SerializeObject(toAccount, new AccountConverter(false)) },
        };
        parameters.AddOptionalParameter("subAcct", subAccountName);
        parameters.AddOptionalParameter("instId", fromInstrumentId);
        parameters.AddOptionalParameter("toInstId", toInstrumentId);

        return await SendOKXSingleRequest<OkxTransferResponse>(RootClient.GetUri(Endpoints_V5_Asset_Transfer), HttpMethod.Post, ct, signed: true, bodyParameters: parameters).ConfigureAwait(false);
    }

    // TODO: Get funds transfer state

    /// <summary>
    /// Query the billing record, you can get the latest 1 month historical data
    /// </summary>
    /// <param name="currency">Currency</param>
    /// <param name="type">Bill type</param>
    /// <param name="after">Pagination of data to return records earlier than the requested ts, Unix timestamp format in milliseconds, e.g. 1597026383085</param>
    /// <param name="before">Pagination of data to return records newer than the requested ts, Unix timestamp format in milliseconds, e.g. 1597026383085</param>
    /// <param name="limit">Number of results per request. The maximum is 100; the default is 100.</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns></returns>
    public virtual async Task<RestCallResult<IEnumerable<OkxFundingBill>>> GetFundingBillDetailsAsync(
        string currency = null,
        OkxFundingBillType? type = null,
        long? after = null,
        long? before = null,
        int limit = 100,
        CancellationToken ct = default)
    {
        limit.ValidateIntBetween(nameof(limit), 1, 100);
        var parameters = new Dictionary<string, object>();
        parameters.AddOptionalParameter("ccy", currency);
        parameters.AddOptionalParameter("type", JsonConvert.SerializeObject(type, new FundingBillTypeConverter(false)));
        parameters.AddOptionalParameter("after", after?.ToString(OkxGlobals.OkxCultureInfo));
        parameters.AddOptionalParameter("before", before?.ToString(OkxGlobals.OkxCultureInfo));
        parameters.AddOptionalParameter("limit", limit.ToString(OkxGlobals.OkxCultureInfo));

        return await SendOKXRequest<IEnumerable<OkxFundingBill>>(RootClient.GetUri(Endpoints_V5_Asset_Bills), HttpMethod.Get, ct, signed: true, queryParameters: parameters).ConfigureAwait(false);
    }

    /// <summary>
    /// Users can create up to 10,000 different invoices within 24 hours.
    /// </summary>
    /// <param name="currency">Currency</param>
    /// <param name="amount">deposit amount between 0.000001 - 0.1</param>
    /// <param name="account">Receiving account</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns></returns>
    public virtual async Task<RestCallResult<IEnumerable<OkxLightningDeposit>>> GetLightningDepositsAsync(
        string currency,
        decimal amount,
        OkxLightningDepositAccount? account = null,
        CancellationToken ct = default)
    {
        var parameters = new Dictionary<string, object>
        {
            { "ccy", currency },
            { "amt", amount.ToString(OkxGlobals.OkxCultureInfo) },
        };
        parameters.AddOptionalParameter("to", JsonConvert.SerializeObject(account, new LightningDepositAccountConverter(false)));

        return await SendOKXRequest<IEnumerable<OkxLightningDeposit>>(RootClient.GetUri(Endpoints_V5_Asset_DepositLightning), HttpMethod.Get, ct, signed: true, queryParameters: parameters).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieve the deposit addresses of currencies, including previously-used addresses.
    /// </summary>
    /// <param name="currency">Currency</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns></returns>
    public virtual async Task<RestCallResult<IEnumerable<OkxDepositAddress>>> GetDepositAddressAsync(string currency = null, CancellationToken ct = default)
    {
        var parameters = new Dictionary<string, object> {
            { "ccy", currency },
        };

        return await SendOKXRequest<IEnumerable<OkxDepositAddress>>(RootClient.GetUri(Endpoints_V5_Asset_DepositAddress), HttpMethod.Get, ct, signed: true, queryParameters: parameters).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieve the deposit history of all currencies, up to 100 recent records in a year.
    /// </summary>
    /// <param name="currency">Currency</param>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="state">State</param>
    /// <param name="after">Pagination of data to return records earlier than the requested ts, Unix timestamp format in milliseconds, e.g. 1597026383085</param>
    /// <param name="before">Pagination of data to return records newer than the requested ts, Unix timestamp format in milliseconds, e.g. 1597026383085</param>
    /// <param name="limit">Number of results per request. The maximum is 100; the default is 100.</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns></returns>
    public virtual async Task<RestCallResult<IEnumerable<OkxDepositHistory>>> GetDepositHistoryAsync(
        string currency = null,
        string transactionId = null,
        OkxDepositState? state = null,
        long? after = null,
        long? before = null,
        int limit = 100,
        CancellationToken ct = default)
    {
        limit.ValidateIntBetween(nameof(limit), 1, 100);
        var parameters = new Dictionary<string, object>();
        parameters.AddOptionalParameter("ccy", currency);
        parameters.AddOptionalParameter("txId", transactionId);
        parameters.AddOptionalParameter("state", JsonConvert.SerializeObject(state, new DepositStateConverter(false)));
        parameters.AddOptionalParameter("after", after?.ToString(OkxGlobals.OkxCultureInfo));
        parameters.AddOptionalParameter("before", before?.ToString(OkxGlobals.OkxCultureInfo));
        parameters.AddOptionalParameter("limit", limit.ToString(OkxGlobals.OkxCultureInfo));

        return await SendOKXRequest<IEnumerable<OkxDepositHistory>>(RootClient.GetUri(Endpoints_V5_Asset_DepositHistory), HttpMethod.Get, ct, signed: true, queryParameters: parameters).ConfigureAwait(false);
    }

    /// <summary>
    /// Withdrawal of tokens.
    /// </summary>
    /// <param name="currency">Currency</param>
    /// <param name="amount">Amount</param>
    /// <param name="destination">Withdrawal destination address</param>
    /// <param name="toAddress">Verified digital currency address, email or mobile number. Some digital currency addresses are formatted as 'address+tag', e.g. 'ARDOR-7JF3-8F2E-QUWZ-CAN7F:123456'</param>
    /// <param name="password">Trade password</param>
    /// <param name="fee">Transaction fee</param>
    /// <param name="chain">Chain name. There are multiple chains under some currencies, such as USDT has USDT-ERC20, USDT-TRC20, and USDT-Omni. If this parameter is not filled in because it is not available, it will default to the main chain.</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns></returns>
    public virtual async Task<RestCallResult<OkxWithdrawalResponse>> WithdrawAsync(
        string currency,
        decimal amount,
        OkxWithdrawalDestination destination,
        string toAddress,
        string password,
        decimal fee,
        string chain = null,
        CancellationToken ct = default)
    {
        var parameters = new Dictionary<string, object> {
            { "ccy",currency},
            { "amt",amount.ToString(OkxGlobals.OkxCultureInfo)},
            { "dest", JsonConvert.SerializeObject(destination, new WithdrawalDestinationConverter(false)) },
            { "toAddr",toAddress},
            { "pwd",password},
            { "fee",fee   .ToString(OkxGlobals.OkxCultureInfo)},
        };
        parameters.AddOptionalParameter("chain", chain);

        return await SendOKXSingleRequest<OkxWithdrawalResponse>(RootClient.GetUri(Endpoints_V5_Asset_Withdrawal), HttpMethod.Post, ct, signed: true, bodyParameters: parameters).ConfigureAwait(false);
    }

    /// <summary>
    /// The maximum withdrawal amount is 0.1 BTC per request, and 1 BTC in 24 hours. The minimum withdrawal amount is approximately 0.000001 BTC. Sub-account does not support withdrawal.
    /// </summary>
    /// <param name="currency">Token symbol. Currently only BTC is supported.</param>
    /// <param name="invoice">Invoice text</param>
    /// <param name="memo">Lightning withdrawal memo</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns></returns>
    public virtual async Task<RestCallResult<OkxLightningWithdrawal>> GetLightningWithdrawalsAsync(
        string currency,
        string invoice,
        string memo = null,
        CancellationToken ct = default)
    {
        var parameters = new Dictionary<string, object>
        {
            { "ccy", currency },
            { "invoice", invoice },
        };
        parameters.AddOptionalParameter("memo", memo);

        return await SendOKXSingleRequest<OkxLightningWithdrawal>(RootClient.GetUri(Endpoints_V5_Asset_WithdrawalLightning), HttpMethod.Get, ct, signed: true, queryParameters: parameters).ConfigureAwait(false);
    }


    /// <summary>
    /// Cancel withdrawal
    /// You can cancel normal withdrawal requests, but you cannot cancel withdrawal requests on Lightning.
    /// Rate Limit: 6 requests per second
    /// </summary>
    /// <param name="withdrawalId">Withdrawal ID</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns></returns>
    public virtual async Task<RestCallResult<OkxWithdrawalId>> CancelWithdrawalAsync(string withdrawalId, CancellationToken ct = default)
    {
        var parameters = new Dictionary<string, object> {
            { "wdId",withdrawalId},
        };

        return await SendOKXSingleRequest<OkxWithdrawalId>(RootClient.GetUri(Endpoints_V5_Asset_WithdrawalCancel), HttpMethod.Post, ct, signed: true, bodyParameters: parameters).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieve the withdrawal records according to the currency, withdrawal status, and time range in reverse chronological order. The 100 most recent records are returned by default.
    /// </summary>
    /// <param name="currency">Currency</param>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="state">State</param>
    /// <param name="after">Pagination of data to return records earlier than the requested ts, Unix timestamp format in milliseconds, e.g. 1597026383085</param>
    /// <param name="before">Pagination of data to return records newer than the requested ts, Unix timestamp format in milliseconds, e.g. 1597026383085</param>
    /// <param name="limit">Number of results per request. The maximum is 100; the default is 100.</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns></returns>
    public virtual async Task<RestCallResult<IEnumerable<OkxWithdrawalHistory>>> GetWithdrawalHistoryAsync(
        string currency = null,
        string transactionId = null,
        OkxWithdrawalState? state = null,
        long? after = null,
        long? before = null,
        int limit = 100,
        CancellationToken ct = default)
    {
        limit.ValidateIntBetween(nameof(limit), 1, 100);
        var parameters = new Dictionary<string, object>();
        parameters.AddOptionalParameter("ccy", currency);
        parameters.AddOptionalParameter("txId", transactionId);
        parameters.AddOptionalParameter("state", JsonConvert.SerializeObject(state, new WithdrawalStateConverter(false)));
        parameters.AddOptionalParameter("after", after?.ToString(OkxGlobals.OkxCultureInfo));
        parameters.AddOptionalParameter("before", before?.ToString(OkxGlobals.OkxCultureInfo));
        parameters.AddOptionalParameter("limit", limit.ToString(OkxGlobals.OkxCultureInfo));

        return await SendOKXRequest<IEnumerable<OkxWithdrawalHistory>>(RootClient.GetUri(Endpoints_V5_Asset_WithdrawalHistory), HttpMethod.Get, ct, signed: true, queryParameters: parameters).ConfigureAwait(false);
    }

    // TODO: Small assets convert

    /// <summary>
    /// Get saving balance
    /// Only the assets in the funding account can be used for saving.
    /// Rate Limit: 6 requests per second
    /// </summary>
    /// <param name="currency">Currency, e.g. BTC</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns></returns>
    public virtual async Task<RestCallResult<IEnumerable<OkxSavingBalance>>> GetSavingBalancesAsync(string currency = null, CancellationToken ct = default)
    {
        var parameters = new Dictionary<string, object>();
        parameters.AddOptionalParameter("ccy", currency);

        return await SendOKXRequest<IEnumerable<OkxSavingBalance>>(RootClient.GetUri(Endpoints_V5_Asset_SavingBalance), HttpMethod.Get, ct, signed: true, queryParameters: parameters).ConfigureAwait(false);
    }

    public virtual async Task<RestCallResult<OkxSavingActionResponse>> SavingPurchaseRedemptionAsync(
        string currency,
        decimal amount,
        OkxSavingActionSide side,
        decimal? rate = null,
        CancellationToken ct = default)
    {
        var parameters = new Dictionary<string, object> {
            { "ccy",currency},
            { "amt",amount.ToString(OkxGlobals.OkxCultureInfo)},
            { "side", JsonConvert.SerializeObject(side, new SavingActionSideConverter(false)) },
        };
        parameters.AddOptionalParameter("rate", rate?.ToString(OkxGlobals.OkxCultureInfo));

        return await SendOKXSingleRequest<OkxSavingActionResponse>(RootClient.GetUri(Endpoints_V5_Asset_SavingPurchaseRedempt), HttpMethod.Post, ct, signed: true, bodyParameters: parameters).ConfigureAwait(false);
    }

    // TODO: Set lending rate
    // TODO: Get lending history
    // TODO: Get public borrow info (public)
    // TODO: Get public borrow history (public)
    #endregion

}