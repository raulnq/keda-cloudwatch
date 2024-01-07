import http from 'k6/http';
import { sleep } from 'k6';
export const options = {
    vus: 16,
    duration: '600s',
};
export default function () {
    http.get('<MY_URL>');
    sleep(1);
}