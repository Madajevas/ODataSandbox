import { OData } from "npm:@odata/client";

const client = OData.New4(
    {
        metadataUri: "http://localhost:5093/odata/$metadata",
        credential: {
            username: "testing",
            password: "testing"
        },
        commonHeaders: {
            // lib request response to be in json format
            // but is not prepared to deal with it
            // removing accept return boundary separated results
            'Accept': undefined
        }
    });

const results = await client.execBatchRequests(
    [
        client.newBatchRequest({
            collection: "WeatherForecast",
            method: "GET"
          }),
        client.newBatchRequest({
            collection: "WeatherForecast",
            method: "GET"
          }),
    ]);

for (let result of results) {
    console.log(result.status);
    console.log(await result.json());
}
