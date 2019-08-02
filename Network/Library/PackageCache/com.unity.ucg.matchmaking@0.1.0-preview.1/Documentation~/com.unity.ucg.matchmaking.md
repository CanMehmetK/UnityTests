# About the **Multiplay Matchmaking Client**
Use the Multiplay Matchmaking Client package to quickly integrate your project with Unity's cloud-based Matchmaking service (see https://unity3d.com/connectedgames for more info).

This package provides a reference Unity (C#) implementation of a matchmaking client which calls the Matchmaking service's web APIs, as well as the latest matchmaking data models which can be used to extend the built-in matchmaking client or build your own.

# Installing the **Multiplay Matchmaking Client**
To install this package, follow the instructions in the [Package Manager documentation](https://docs.unity3d.com/Packages/com.unity.package-manager-ui@latest/index.html). 

# Using the **Multiplay Matchmaking Client**
[Note - this documentation will be moved when proper service API documentation has been published]

The Unity matchmaking API consists of two main calls:
*  /request
*  /assignment

The ```/request``` POST call submits a request to start looking for a match for the submitted client(s) with the submitted request properties (such as game mode, etc.)

The ```/assignment``` GET call is a long TTL call which returns an *Assignment* when the matchmaking service has found a match.  It can also return an *AssignmentError* if the mathcmaking service is unable to find a match for the request.

The package currently includes a simple implementation of a state machine to track the state of your matchmaking calls, and the data structures and serialization / deserialization code necessary to send a properly-formed Request call and decode the response of a properly-formed Assignment call.


# Technical details
## Requirements
This version of the **Multiplay Matchmaking Client** is compatible with the following versions of the Unity Editor:
* 2018.1 and later (recommended)


## Known limitations
 The **Multiplay Matchmaking Client** version **0.1.0-preview** includes the following known limitations:
* Minimal error handling
* Not compatible with pure DOTS runtime (reliant on certain "classic" Unity features such as UnityWebRequest)
* The current implementation works via call-backs; future versions may inlcude other optional implementations


## Package contents
|Location|Description|
|---|---|
|`\Runtime\`|Contains the runtime code|
|`\Samples\`|Contains sample code|


## Document revision history
|Date|Reason|
|---|---|
|April 10, 2019|Document created for package version 0.1.0-preview|
