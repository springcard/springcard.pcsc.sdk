# SmartTap

* Google Pay Smart Tap: https://developers.google.com/pay/smart-tap/
* Google Pay API for Passes : https://developers.google.com/pay/passes/

## Vocabulary and concepts

### Vocabulary

* **Collector** or **merchant**: a company that contracts with Google to use the Smart Tap services.

### Non-implemented features

SpringCard's SmartTap library has the following limitations:

- Payment is not implemented (only VAS aka Smart Tap),
- PUSH DATA is not implemented (only GET DATA).

### Security of the transaction

0. The merchant is assigned a <u>collector ID</u>, associated to a <u>long-term ECC key pair</u>. The public part of the long-term ECC key pair has to be provided to Google services through Google Pay API for Passes Merchant Center, and is then distributed onto the mobiles with the passes. The private part has to be injected into all the merchant's terminals, together with the collector ID.
1. Both the terminal and the mobile generate an <u>ephemeral ECC key pair</u> and a <u>nonce</u>, which are different for every transactions,
2. The **mobile** sends its nonce to the terminal in response to the SELECT OSE command,
3. The **terminal** sends its collector ID, its nonce and its <u>ephemeral public key</u> to the mobile in the NEGOTIATE SECURE SESSION command; this command also includes a <u>signature</u>, which is computed over the preceding data using the collector's <u>long-term ECC private key</u>.
4. Thanks to the merchant's <u>long-term ECC public</u> key that it already knows, the **mobile** is able to verify the signature, i.e. to make sure that the terminal actually belong to the merchant,
5. The **mobile** sends its <u>ephemeral ECC public key</u> to the terminal in response,
6. Both the terminal and the mobile run the ECDH algorithm to deduce a <u>shared secret</u> from the exchanged public keys, and then run HKDF with HMAC-SHA-256 over the shared secret and the nonces to generate an AES key and an HMAC key,
7. The terminal issues a GET DATA command (in plain) to tell the mobile which data it is looking for,
8. The device transmits the data ciphered with AES-CTR (using the shared AES key) and authenticated with HMAC-SHA-256 (using the shared HMAC key),
9. The terminal verifies the HMAC and deciphers the cryptogram to retrieve the data.

### Key rotation

Every merchant's long-term ECC key pair is associated to a 32-bit version number.

Google advices to generate and distribute a new key pair at least every 30 days. This implies that it must be easy to inject new private keys in the terminals periodically.

## Merchant road-book

### Create a Loyalty scheme

#### Create the long-term private key

```shell
openssl ecparam -name prime256v1 -genkey -noout -out SMART_TAP_KEY.pri.pem
openssl ec -in SMART_TAP_KEY.pri.pem -pubout -out SMART_TAP_KEY.pub.pem
```

#### Declare a new issuer account

* Go to https://pay.google.com/gp/m/issuer/create/
* Enter your details
* In the "Authentication key" section, provide the file `SMART_TAP_KEY.pub.pem` as the public key
* Set "Key version" to 1
* Click "Create"
* Come back to the account details, and get the Merchant ID (or Collector ID).

#### Use Google Pay API for passes to create passes

Reference documentation: https://developers.google.com/pay/passes/

Follow the **basic setup** part to [Get access to REST API](https://developers.google.com/pay/passes/guides/get-started/basic-setup/get-access-to-rest-api).





## The SmartTapRdr application

### Demo passes

- [Google demo pass 1](https://androidpay.google.com/u/0/a/save/eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJnb29nbGUiLCJvcmlnaW5zIjpbImh0dHA6Ly9sb2NhbGhvc3Q6ODA4MCJdLCJpc3MiOiJzMmdvb2dsZXBheS1hcGlAc3VubGl0LXBpeGVsLTE5NzQyMS5nb29nbGUuY29tLmlhbS5nc2VydmljZWFjY291bnQuY29tIiwiaWF0IjoxNTI5OTU2MDcwLCJ0eXAiOiJzYXZldG9hbmRyb2lkcGF5IiwicGF5bG9hZCI6eyJsb3lhbHR5T2JqZWN0cyI6W3siY2xhc3NJZCI6IjMyNjUzMjAxMTE2NDE5NTYxODMuMDYxOV9nb29nbGVEZW1vVGVzdCIsInN0YXRlIjoiYWN0aXZlIiwiaWQiOiIzMjY1MzIwMTExNjQxOTU2MTgzLjA2MTlfZ29vZ2xlRGVtb1Rlc3Qtb2JqMDEifV19fQ.C_x3ZzDgh77rr9y0ITIA53aWd3375_rauxcb1aBdyocI1Tes-fzqIxuohGeOvBMWYsnju1069lz9qhEu1IqP3GjTcxXyo96VXh6oo3bm4_UiI2VmbX7Dld6CCGdmeqfnUhLZd_nyZdIGk0rGFVqwLi-0XgPR6LI3eqPSz3Io1_6WLWbKBJn6bKr9WxM64E1O-8zy2Ky9UuQlod-MoJR5IuO_iOnzmK7D3WnJ-9eOIZzxpRq6XbQOcPoJtC46EVoRj3JH3xEWgU4HlDB_9vFxDBx2qHQoHeOgh59czdhXGUti0Vg0bfzhZ5fcFGDGsETLhvJrP1X5hVD57CruyCM5hw) (get secure data)
- [Google demo pass 2](https://androidpay.google.com/u/4/a/save/eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9. eyJhdWQiOiJnb29nbGUiLCJvcmlnaW5zIjpbImh0dHA6Ly9sb2NhbGhvc3Q6ODA4MCJdLCJpc3MiOiJzMmdvb2dsZXBheS1hcGlAc3VubGl0LXBpeGVsLTE5NzQyMS5nb29nbGUuY29tLmlhbS5nc2VydmljZWFjY291bnQuY29tIiwiaWF0IjoxNTQ2NjQ0MzI4LCJ0eXAiOiJzYXZldG9hbmRyb2lkcGF5IiwicGF5bG9hZCI6eyJsb3lhbHR5T2JqZWN0cyI6W3siaWQiOiIzMjY1MzIwMTExNjQxOTU2MTgzLjIwMTkwMTA0X2dvb2dsZV9seV9kZW1vX3Bhc3Mtb2JqMDEifV19fQ.Bb4qZGYbkmkClCo0R5xiPk6wNXD9YgBIJOJ5O2LhXVFx25cgPPbGUeCvQb7E-jpbaj2Cgm0tglQ8OStQCWH_fW6Rg0M27f7xDONMdcWBIgaUUidYB8PbRQNLYR0u5BP6Ilo-_LLzS427JJTAttuwmmWVPJX4tELBd0ghVtF4R8O0wEg47JTEvl1Hqs_esWUpoyIHbGKGlA636r6aJi8n7vFEcBA_JxTuQyBVrEu9bfe4II67QZjz6HpLr_TBeIB0di4s5gx47yzfBXCNvk6N4YXOByJFlCG5CP9xkSnWyZeyCs3mkpOlbkCNrTovzFV0_jnqHiyALsHPtPoNhblpbw) (get additional secure data)


### Typical configurations

#### Get data

#### Securely get data

#### Push data with authentication

## The SpringCard.PCSC.CardHelpers.GoogleVAS class library

