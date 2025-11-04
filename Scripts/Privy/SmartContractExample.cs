using UnityEngine;
using UnityEngine.UI;
using Privy;

/// <summary>
/// Example script demonstrating smart contract interactions with Privy
/// </summary>
public class SmartContractExample : MonoBehaviour
{
    [Header("UI References")]
    public Button sendTransactionButton;
    public Button callContractButton;
    public Button getBalanceButton;
    public Button transferTokensButton;

    public InputField contractAddressField;
    public InputField functionDataField;
    public InputField valueField;
    public InputField recipientAddressField;
    public InputField tokenAmountField;

    public Text resultText;
    public Text walletAddressText;
    public Text balanceText;

    private PrivyController privyController;

    void Start()
    {
        privyController = FindFirstObjectByType<PrivyController>();

        // Set up button listeners
        if (sendTransactionButton != null)
            sendTransactionButton.onClick.AddListener(SendTransaction);

        if (callContractButton != null)
            callContractButton.onClick.AddListener(CallContract);

        if (getBalanceButton != null)
            getBalanceButton.onClick.AddListener(GetWalletBalance);

        if (transferTokensButton != null)
            transferTokensButton.onClick.AddListener(TransferTokens);

        // Set up Privy event listeners
        if (privyController != null)
        {
            privyController.onTransactionSent.AddListener(OnTransactionSent);
            privyController.onContractCallResult.AddListener(OnContractCallResult);
        }

        // Update wallet info
        UpdateWalletInfo();
    }

    /// <summary>
    /// Send a transaction to a smart contract
    /// </summary>
    public async void SendTransaction()
    {
        if (privyController == null)
        {
            Debug.LogError("PrivyController not found!");
            return;
        }

        string contractAddress = contractAddressField?.text;
        string functionData = functionDataField?.text;
        string value = valueField?.text;

        if (string.IsNullOrEmpty(contractAddress))
        {
            Debug.LogError("Contract address is required!");
            return;
        }

        if (string.IsNullOrEmpty(functionData))
        {
            Debug.LogError("Function data is required!");
            return;
        }

        // Convert ETH value to wei if provided
        if (!string.IsNullOrEmpty(value) && decimal.TryParse(value, out decimal ethValue))
        {
            value = SmartContractHelper.EthToWei(ethValue);
        }
        else if (string.IsNullOrEmpty(value))
        {
            value = "0x0";
        }

        privyController.SendTransaction(contractAddress, functionData, value);
    }

    /// <summary>
    /// Call a smart contract function (read-only)
    /// </summary>
    public async void CallContract()
    {
        if (privyController == null)
        {
            Debug.LogError("PrivyController not found!");
            return;
        }

        string contractAddress = contractAddressField?.text;
        string functionData = functionDataField?.text;

        if (string.IsNullOrEmpty(contractAddress))
        {
            Debug.LogError("Contract address is required!");
            return;
        }

        if (string.IsNullOrEmpty(functionData))
        {
            Debug.LogError("Function data is required!");
            return;
        }

        privyController.CallContract(contractAddress, functionData);
    }

    /// <summary>
    /// Get the wallet's ETH balance
    /// </summary>
    public async void GetWalletBalance()
    {
        if (privyController == null)
        {
            Debug.LogError("PrivyController not found!");
            return;
        }

        string balanceHex = await privyController.GetBalance();
        if (!string.IsNullOrEmpty(balanceHex))
        {
            decimal balanceEth = SmartContractHelper.WeiToEth(balanceHex);
            if (balanceText != null)
                balanceText.text = $"Balance: {balanceEth:F6} ETH";

            Debug.Log($"Wallet balance: {balanceEth:F6} ETH");
        }
    }

    /// <summary>
    /// Transfer ERC-20 tokens
    /// </summary>
    public async void TransferTokens()
    {
        if (privyController == null)
        {
            Debug.LogError("PrivyController not found!");
            return;
        }

        string contractAddress = contractAddressField?.text;
        string recipientAddress = recipientAddressField?.text;
        string tokenAmount = tokenAmountField?.text;

        if (string.IsNullOrEmpty(contractAddress))
        {
            Debug.LogError("Contract address is required!");
            return;
        }

        if (string.IsNullOrEmpty(recipientAddress))
        {
            Debug.LogError("Recipient address is required!");
            return;
        }

        if (string.IsNullOrEmpty(tokenAmount) || !decimal.TryParse(tokenAmount, out decimal amount))
        {
            Debug.LogError("Valid token amount is required!");
            return;
        }

        // Create ERC-20 transfer function data
        string transferData = SmartContractHelper.CreateERC20TransferData(recipientAddress, amount);

        // Send transaction with no ETH value (just calling contract function)
        privyController.SendTransaction(contractAddress, transferData, "0x0");
    }

    /// <summary>
    /// Get ERC-20 token balance
    /// </summary>
    public async void GetTokenBalance(string tokenContractAddress)
    {
        if (privyController == null)
        {
            Debug.LogError("PrivyController not found!");
            return;
        }

        // Get wallet address
        var privyUser = await PrivyManager.Instance.GetUser();
        if (privyUser?.EmbeddedWallets?.Length > 0)
        {
            string walletAddress = privyUser.EmbeddedWallets[0].Address;

            // Create balanceOf function data
            string balanceData = SmartContractHelper.CreateERC20BalanceOfData(walletAddress);

            // Call contract
            privyController.CallContract(tokenContractAddress, balanceData);
        }
    }

    /// <summary>
    /// Update wallet information display
    /// </summary>
    private async void UpdateWalletInfo()
    {
        try
        {
            var privyUser = await PrivyManager.Instance.GetUser();
            if (privyUser?.EmbeddedWallets?.Length > 0)
            {
                string walletAddress = privyUser.EmbeddedWallets[0].Address;
                if (walletAddressText != null)
                    walletAddressText.text = $"Wallet: {walletAddress}";

                // Get balance
                GetWalletBalance();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error updating wallet info: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle transaction sent event
    /// </summary>
    private void OnTransactionSent(string transactionHash)
    {
        if (resultText != null)
            resultText.text = $"Transaction sent: {transactionHash}";

        Debug.Log($"Transaction sent: {transactionHash}");
    }

    /// <summary>
    /// Handle contract call result event
    /// </summary>
    private void OnContractCallResult(string result)
    {
        if (resultText != null)
            resultText.text = $"Contract call result: {result}";

        Debug.Log($"Contract call result: {result}");
    }

    /// <summary>
    /// Example: Interact with a simple contract
    /// </summary>
    public async void ExampleContractInteraction()
    {
        // Example contract address (replace with your actual contract)
        string contractAddress = "0x1234567890123456789012345678901234567890";

        // Example: Call a function that returns a string
        // Function signature: getMessage()
        string functionData = "0x12345678"; // Replace with actual function selector

        // Call the contract
        privyController.CallContract(contractAddress, functionData);

        // Example: Send a transaction to set a value
        // Function signature: setMessage(string)
        string setMessageData = "0x87654321"; // Replace with actual function selector + encoded parameters

        // Send transaction (no ETH value)
        privyController.SendTransaction(contractAddress, setMessageData, "0x0");
    }
}
