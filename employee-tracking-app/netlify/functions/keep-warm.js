const apiUrl = (process.env.API_URL
  || "https://employee-tracking-api-f3cgd5cgdghcg2c7.westus3-01.azurewebsites.net")
  .replace(/\/$/, "");

export default async () => {
  const target = `${apiUrl}/swagger/index.html`;
  try {
    const response = await fetch(target, { signal: AbortSignal.timeout(25000) });
    console.log(`keep-warm ${response.status} ${target}`);
    return new Response(JSON.stringify({ ok: response.ok, status: response.status }), {
      status: response.ok ? 200 : 502,
      headers: { "content-type": "application/json" }
    });
  } catch (error) {
    console.log(`keep-warm failed ${target}`, error);
    return new Response(JSON.stringify({ ok: false }), {
      status: 502,
      headers: { "content-type": "application/json" }
    });
  }
};
