import http from 'k6/http';

export const options = {
    vus: 10,
    duration: '20s',
    insecureSkipTLSVerify: true
}

const baseUrl = 'https://localhost:7232';

export default function () {
    let r1 = http.get(`${baseUrl}/timed-service?delay=1000`);
    console.log("Load GET:", r1.body);
}