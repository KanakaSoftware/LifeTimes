import http from 'k6/http';
import { sleep } from 'k6';

export const options = {
    insecureSkipTLSVerify: true
}

const baseUrl = 'https://localhost:7232';

export default function () {
    let r1 = http.get(`${baseUrl}/timed-service`);
    console.log("GET 1:", r1.body);
    let r2 = http.asyncRequest("GET", `${baseUrl}/timed-service?delay=10`);
    sleep(10);
    let r3 = http.get(`${baseUrl}/timed-service`);
    let r4 = http.get(`${baseUrl}/timed-service`);
    console.log("GET 3:", r3.body);
    console.log("GET 4:", r4.body);
    r2.then((res) => {
            console.log("GET 2:", res.body);
        }
    );
}