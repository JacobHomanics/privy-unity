# Privy Unity SDK

[![Unity Version](https://img.shields.io/badge/Unity-6000.0%2B-blue.svg)](https://unity3d.com/)
[![License](https://img.shields.io/badge/license-See%20LICENSE.md-green.svg)](LICENSE.md)

Privy but in Unity! A comprehensive Unity SDK for integrating Privy authentication and embedded wallet functionality into your Unity games and applications.

## Features

- 🔐 **Email Authentication** - Passwordless email login with verification codes
- 🔑 **OAuth Support** - Sign in with Apple and other OAuth providers
- 💼 **Embedded Wallets** - Create and manage Ethereum and Solana wallets
- 📱 **Multi-Platform Support** - iOS, WebGL, and Editor support
- 🔗 **Smart Contract Integration** - Send transactions, call contracts, and interact with the blockchain
- 💰 **Wallet Management** - Get balances, sign messages, and manage transactions
- 🎮 **Unity Integration** - Prefabs and scripts ready to use in your Unity projects

## Requirements

- Unity 6000.0 or higher
- Privy account with App ID and Client IDs
- For iOS: Xcode and iOS development setup
- For WebGL: Unity WebGL build support

## Installation

1. Add this package to your Unity project via the Package Manager
2. Import the package into your Unity project
3. Configure your Privy credentials (see Configuration section)

## Quick Start

### 1. Add PrivyController to Your Scene

1. Drag the `Privy.prefab` from the `Prefabs` folder into your scene
2. Or create an empty GameObject and add the `PrivyController` component

### 2. Configure Privy Settings

In the `PrivyController` component, set:

- **App ID**: Your Privy App ID
- **Mobile Client ID**: Your mobile client ID (used in Editor)
- **Web Client ID**: Your web client ID (used in WebGL builds)
- **Create Wallet On Login**: Whether to automatically create a wallet when user logs in

### 3. Send Verification Code

```csharp
PrivyController privyController = FindFirstObjectByType<PrivyController>();
await privyController.SendCode("user@example.com");
```

### 4. Login with Code

```csharp
await privyController.LoginWithCode("user@example.com", "123456");
```

### 5. Access User and Wallet

```csharp
PrivyUser user = await PrivyManager.Instance.GetUser();
IEmbeddedEthereumWallet wallet = user.EmbeddedWallets[0];
string address = wallet.Address;
```

## Configuration

### Privy Dashboard Setup

1. Create a Privy account at [privy.io](https://privy.io)
2. Create a new app in the Privy dashboard
3. Get your **App ID** and **Client IDs**
4. Configure allowed app identifiers for your platforms

### Unity Configuration

In your `PrivyController`:

- **App ID**: Your Privy App ID from the dashboard
- **Mobile Client ID**: OAuth client ID for mobile platforms (iOS)
- **Web Client ID**: OAuth client ID for WebGL builds

## Usage Examples

### Email Authentication

```csharp
public class LoginExample : MonoBehaviour
{
    private PrivyController privyController;

    void Start()
    {
        privyController = FindFirstObjectByType<PrivyController>();
    }

    public async void OnSendCodeClicked()
    {
        await privyController.SendCode("user@example.com");
    }

    public async void OnLoginClicked(string email, string code)
    {
        await privyController.LoginWithCode(email, code);
    }
}
```

### Sign Messages

```csharp
PrivyUser user = await PrivyManager.Instance.GetUser();
IEmbeddedEthereumWallet wallet = user.EmbeddedWallets[0];

var rpcRequest = new RpcRequest
{
    Method = "personal_sign",
    Params = new string[] { "Hello, World!", wallet.Address }
};

RpcResponse response = await wallet.RpcProvider.Request(rpcRequest);
Debug.Log($"Signature: {response.Data}");
```

### Send Transactions

```csharp
// Send ETH
privyController.SendTransaction(
    to: "0xRecipientAddress",
    data: "0x",
    value: "0x186a0" // 100000 wei in hex
);

// Call a smart contract function
privyController.SendTransaction(
    to: "0xContractAddress",
    data: "0xFunctionData",
    value: "0x0"
);
```

### Get Wallet Balance

```csharp
string balanceHex = await privyController.GetBalance();
decimal balanceEth = SmartContractHelper.WeiToEth(balanceHex);
Debug.Log($"Balance: {balanceEth} ETH");
```

### Interact with Smart Contracts

```csharp
// Read from contract (call)
privyController.CallContract(
    to: "0xContractAddress",
    data: "0xFunctionData"
);

// Write to contract (transaction)
privyController.SendTransaction(
    to: "0xContractAddress",
    data: "0xFunctionData",
    value: "0x0"
);
```

### Chain Validation

```csharp
// Validate chain before sending transaction
bool isValid = await privyController.ValidateChain("0x1"); // Ethereum mainnet

if (isValid)
{
    privyController.SendTransaction(to, data, value);
}

// Or use the convenience method
privyController.SendTransactionWithChainValidation(
    to: "0xContractAddress",
    data: "0xFunctionData",
    value: "0x0",
    expectedChainId: "0x1"
);
```

## Prefabs

The SDK includes several ready-to-use prefabs:

- **Privy.prefab** - Main Privy controller with initialization
- **Privy User Id Text.prefab** - Displays the current user ID
- **Privy User Wallet Text.prefab** - Displays the wallet address
- **Privy User Wallet Balance.prefab** - Displays the wallet balance

## Platform Support

### iOS

- Native Apple Sign In support
- ASWebAuthenticationSession for OAuth flows
- Full embedded wallet functionality

### WebGL

- Popup-based OAuth flows
- Iframe-based wallet interactions
- Full blockchain interaction support

### Editor

- Development and testing support
- Uses mobile client ID for OAuth flows

## API Reference

### PrivyManager

Main entry point for Privy functionality.

```csharp
// Initialize Privy
PrivyManager.Initialize(config);

// Get current user
PrivyUser user = await PrivyManager.Instance.GetUser();

// Email authentication
bool success = await PrivyManager.Instance.Email.SendCode(email);
AuthState state = await PrivyManager.Instance.Email.LoginWithCode(email, code);
```

### PrivyUser

Represents an authenticated Privy user.

```csharp
// Get user ID
string userId = user.Id;

// Get embedded wallets
IEmbeddedEthereumWallet[] wallets = user.EmbeddedWallets;

// Create a new wallet
await user.CreateWallet();
```

### IEmbeddedEthereumWallet

Interface for Ethereum wallet operations.

```csharp
// Get wallet address
string address = wallet.Address;

// Get current chain ID
string chainId = wallet.ChainId;

// Make RPC calls
RpcResponse response = await wallet.RpcProvider.Request(rpcRequest);
```

### RPC Methods

Supported Ethereum RPC methods:

- `personal_sign` - Sign messages
- `eth_sendTransaction` - Send transactions
- `eth_call` - Call contract functions
- `eth_getBalance` - Get wallet balance
- `eth_gasPrice` - Get current gas price
- `eth_signTypedData_v4` - Sign typed data
- And more...

## Demo Scene

Check out the `Demo.unity` scene in the `Scenes` folder for a complete working example of:

- Email authentication flow
- Wallet creation and management
- Smart contract interactions
- Balance checking
- Transaction sending

## Troubleshooting

### "Invalid Privy app ID" Error

- Verify your App ID is correct in the PrivyController
- Ensure the App ID matches your Privy dashboard

### "Invalid app client ID" Error

- Check that your Client IDs are set correctly
- Verify the client IDs match your platform (mobile vs web)

### "App identifier has not been set" Error

- Add your app's bundle identifier to the Privy dashboard
- For iOS: Add your bundle ID
- For WebGL: Add your domain

### Wallet Creation Fails

- Ensure user is authenticated before creating wallet
- Check network connectivity
- Verify Privy dashboard settings allow wallet creation

### OAuth Not Working

- For iOS: Ensure proper entitlements and Info.plist configuration
- For WebGL: Check popup blockers and browser permissions
- Verify OAuth provider is configured in Privy dashboard

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

See [LICENSE.md](LICENSE.md) for details.

## Third Party Notices

See [Third Party Notices.md](Third%20Party%20Notices.md) for information about third-party software used in this package.

## Support

For issues, questions, or feature requests:

- Email: homanicsjake@gmail.com
- Website: https://jacobhomanics.com

## Author

**Jacob Homanics**

- Email: homanicsjake@gmail.com
- Website: https://jacobhomanics.com
