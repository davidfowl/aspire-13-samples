export async function register() {
  if (process.env.NEXT_RUNTIME === 'nodejs') {
    const { NodeSDK } = await import('@opentelemetry/sdk-node');
    const { OTLPTraceExporter } = await import('@opentelemetry/exporter-trace-otlp-grpc');
    const { credentials } = await import('@grpc/grpc-js');

    console.log('[OpenTelemetry] Initializing SDK...');
    console.log('[OpenTelemetry] OTEL_EXPORTER_OTLP_ENDPOINT:', process.env.OTEL_EXPORTER_OTLP_ENDPOINT);
    console.log('[OpenTelemetry] OTEL_SERVICE_NAME:', process.env.OTEL_SERVICE_NAME);

    const isHttps = process.env.OTEL_EXPORTER_OTLP_ENDPOINT?.startsWith('https://');
    const collectorOptions = {
      credentials: !isHttps ? credentials.createInsecure() : credentials.createSsl()
    };

    const sdk = new NodeSDK({
      traceExporter: new OTLPTraceExporter(collectorOptions),
    });

    sdk.start();
    console.log('[OpenTelemetry] SDK started successfully');

    process.on('SIGTERM', () => {
      sdk.shutdown()
        .then(() => console.log('[OpenTelemetry] SDK shut down successfully'))
        .catch((error) => console.error('[OpenTelemetry] Error shutting down SDK', error))
        .finally(() => process.exit(0));
    });
  }
}
