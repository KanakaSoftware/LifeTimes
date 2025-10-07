import http from 'k6/http';

export const options = {
    insecureSkipTLSVerify: true
}

const baseUrl = 'https://localhost:7232';

export default function () {
    let r1 = http.get(`${baseUrl}/conditional-service`);
    console.log("GET 1:", r1.body);
    let r2 = http.get(`${baseUrl}/conditional-service`);
    console.log("GET 2:", r2.body);
    let r3 = http.get(`${baseUrl}/conditional-service`);
    console.log("GET 3:", r3.body);
    let r4 = http.get(`${baseUrl}/conditional-service`);
    console.log("GET 4:", r4.body);
    let r5 = http.get(`${baseUrl}/conditional-service`);
    console.log("GET 5:", r5.body);
    let r6 = http.get(`${baseUrl}/conditional-service`);
    console.log("GET 6:", r6.body);
}