using UnityEngine;
using System;
using System.Numerics;
using System.Text;

/// <summary>
/// Helper class for common smart contract interactions
/// </summary>
public static class SmartContractHelper
{
    /// <summary>
    /// Convert a string to hex format (for function parameters)
    /// </summary>
    public static string StringToHex(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        return "0x" + BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }

    /// <summary>
    /// Convert a number to hex format (for function parameters)
    /// </summary>
    public static string NumberToHex(int number)
    {
        return "0x" + number.ToString("X").ToLower();
    }

    /// <summary>
    /// Convert ETH to Wei (for transaction values)
    /// </summary>
    public static string EthToWei(decimal eth)
    {
        BigInteger wei = (BigInteger)(eth * 1000000000000000000m);
        return "0x" + wei.ToString("X").ToLower();
    }

    /// <summary>
    /// Convert Wei to ETH
    /// </summary>
    public static decimal WeiToEth(string weiHex)
    {
        if (weiHex.StartsWith("0x"))
            weiHex = weiHex.Substring(2);

        BigInteger wei = BigInteger.Parse(weiHex, System.Globalization.NumberStyles.HexNumber);
        return (decimal)wei / 1000000000000000000m;
    }

    /// <summary>
    /// Create function call data for ERC-20 transfer
    /// </summary>
    /// <param name="to">Recipient address</param>
    /// <param name="amount">Amount in tokens (will be converted to smallest unit)</param>
    /// <param name="decimals">Token decimals (default 18)</param>
    public static string CreateERC20TransferData(string to, decimal amount, int decimals = 18)
    {
        // Function selector for transfer(address,uint256)
        string functionSelector = "0xa9059cbb";

        // Remove 0x prefix if present
        if (to.StartsWith("0x"))
            to = to.Substring(2);

        // Convert amount to smallest unit
        BigInteger amountInSmallestUnit = (BigInteger)(amount * (decimal)Math.Pow(10, decimals));

        // Pad address to 32 bytes
        string paddedTo = to.PadLeft(64, '0');

        // Pad amount to 32 bytes
        string paddedAmount = amountInSmallestUnit.ToString("X").PadLeft(64, '0').ToLower();

        return functionSelector + paddedTo + paddedAmount;
    }

    /// <summary>
    /// Create function call data for ERC-20 balanceOf
    /// </summary>
    /// <param name="address">Address to check balance for</param>
    public static string CreateERC20BalanceOfData(string address)
    {
        // Function selector for balanceOf(address)
        string functionSelector = "0x70a08231";

        // Remove 0x prefix if present
        if (address.StartsWith("0x"))
            address = address.Substring(2);

        // Pad address to 32 bytes
        string paddedAddress = address.PadLeft(64, '0');

        return functionSelector + paddedAddress;
    }

    /// <summary>
    /// Create function call data for ERC-20 approve
    /// </summary>
    /// <param name="spender">Address to approve</param>
    /// <param name="amount">Amount to approve</param>
    /// <param name="decimals">Token decimals (default 18)</param>
    public static string CreateERC20ApproveData(string spender, decimal amount, int decimals = 18)
    {
        // Function selector for approve(address,uint256)
        string functionSelector = "0x095ea7b3";

        // Remove 0x prefix if present
        if (spender.StartsWith("0x"))
            spender = spender.Substring(2);

        // Convert amount to smallest unit
        BigInteger amountInSmallestUnit = (BigInteger)(amount * (decimal)Math.Pow(10, decimals));

        // Pad spender address to 32 bytes
        string paddedSpender = spender.PadLeft(64, '0');

        // Pad amount to 32 bytes
        string paddedAmount = amountInSmallestUnit.ToString("X").PadLeft(64, '0').ToLower();

        return functionSelector + paddedSpender + paddedAmount;
    }

    /// <summary>
    /// Parse ERC-20 balance from contract call result
    /// </summary>
    /// <param name="hexResult">Hex result from contract call</param>
    /// <param name="decimals">Token decimals (default 18)</param>
    public static decimal ParseERC20Balance(string hexResult, int decimals = 18)
    {
        if (hexResult.StartsWith("0x"))
            hexResult = hexResult.Substring(2);

        BigInteger balance = BigInteger.Parse(hexResult, System.Globalization.NumberStyles.HexNumber);
        return (decimal)balance / (decimal)Math.Pow(10, decimals);
    }

    /// <summary>
    /// Create function call data for a custom contract function
    /// </summary>
    /// <param name="functionSignature">Function signature (e.g., "transfer(address,uint256)")</param>
    /// <param name="parameters">Function parameters</param>
    public static string CreateCustomFunctionData(string functionSignature, params object[] parameters)
    {
        // This is a simplified version - in practice, you'd want to use a proper ABI encoder
        // For now, this shows the concept of creating function selectors

        // Calculate function selector (first 4 bytes of keccak256 hash)
        string functionSelector = CalculateFunctionSelector(functionSignature);

        // Note: Proper parameter encoding would require ABI encoding logic
        // This is a placeholder for the concept
        Debug.LogWarning("Custom function data creation requires proper ABI encoding implementation");

        return functionSelector;
    }

    /// <summary>
    /// Calculate function selector from function signature
    /// </summary>
    /// <param name="functionSignature">Function signature</param>
    private static string CalculateFunctionSelector(string functionSignature)
    {
        // This is a simplified version - proper implementation would use keccak256
        // For demonstration purposes, returning a placeholder
        Debug.LogWarning("Function selector calculation requires keccak256 implementation");
        return "0x00000000";
    }
}
