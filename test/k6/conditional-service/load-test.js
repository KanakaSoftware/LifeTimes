import http from 'k6/http';

export const options = {
    vus: 3,
    duration: '30s',
    insecureSkipTLSVerify: true
}

const baseUrl = 'https://localhost:7232';

export default function () {
    let r1 = http.get(`${baseUrl}/conditional-service?count=10`);
    console.log("Load GET:", r1.body);
}