<#
.SYNOPSIS
    Publishes a FraudDetected event onto the payment exchange, so the API's
    identity-access.deactivate-on-fraud consumer picks it up and deactivates the user.

.DESCRIPTION
    Nothing in the system publishes FraudDetected yet — it only ever arrives from an external
    fraud-detection service. This script stands in for that service during manual verification,
    using the RabbitMQ management HTTP API (the management plugin must be enabled).

    The envelope shape matches EventEnvelope<T> in HexArch.Messaging.RabbitMQ.Transport exactly
    (camelCase, System.Text.Json "Web" defaults), since that is what the consumer deserializes.

.PARAMETER UserId
    The Guid of the user to report as fraudulent.

.PARAMETER HostName
    RabbitMQ management API host. Defaults to localhost.

.PARAMETER Port
    RabbitMQ management API port (not the AMQP port). Defaults to 15672.

.PARAMETER User
    Management API username. Defaults to admin (matches appsettings.Development.json).

.PARAMETER Password
    Management API password. Defaults to admin.

.PARAMETER ExchangePrefix
    Must match RabbitMq:ExchangePrefix in the running API's configuration. Defaults to hexarch.

.EXAMPLE
    .\scripts\publish-fraud-detected.ps1 -UserId 3fa85f64-5717-4562-b3fc-2c963f66afa6
#>
param(
    [Parameter(Mandatory = $true)]
    [guid]$UserId,

    [string]$HostName = "localhost",
    [int]$Port = 15672,
    [string]$User = "admin",
    [string]$Password = "admin",
    [string]$ExchangePrefix = "hexarch"
)

$ErrorActionPreference = "Stop"

$routingKey = "payment.fraud.detected.v1"
$exchange = "$ExchangePrefix.payment"
$occurredOnUtc = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ")

# Mirrors EventEnvelope<FraudDetected> exactly: camelCase properties, TEvent serialized as its own
# concrete record so every property (not just OccurredOnUtc) makes it onto the wire.
$envelope = [ordered]@{
    eventId       = [guid]::NewGuid().ToString()
    type          = $routingKey
    occurredOnUtc = $occurredOnUtc
    version       = 1
    source        = "fraud-detection"
    data          = [ordered]@{
        userId        = $UserId.ToString()
        occurredOnUtc = $occurredOnUtc
    }
} | ConvertTo-Json -Depth 5 -Compress

$body = @{
    properties        = @{
        delivery_mode = 2
        content_type  = "application/json"
        type          = $routingKey
    }
    routing_key       = $routingKey
    payload           = $envelope
    payload_encoding  = "string"
} | ConvertTo-Json -Depth 5

$credential = [System.Convert]::ToBase64String(
    [System.Text.Encoding]::UTF8.GetBytes("$($User):$($Password)"))

$uri = "http://$($HostName):$($Port)/api/exchanges/%2F/$([Uri]::EscapeDataString($exchange))/publish"

Write-Host "Publishing FraudDetected for user $UserId to exchange '$exchange' (routing key '$routingKey')..."

$response = Invoke-RestMethod -Uri $uri -Method Post -Body $body -ContentType "application/json" `
    -Headers @{ Authorization = "Basic $credential" }

if (-not $response.routed) {
    throw "Published, but nothing routed it: the exchange has no matching binding. " +
        "This means the API never declared the topology (broker was down at startup, or " +
        "-ExchangePrefix doesn't match RabbitMq:ExchangePrefix in the running API)."
}

Write-Host "Published and routed to at least one queue."
