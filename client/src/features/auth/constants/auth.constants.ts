import { API_BASE_URL } from "@/constants/app.constants";

export const GOOGLE_LOGIN_URL = `${API_BASE_URL}/auth/google`;

// ponytail: no request-access endpoint yet, route through the owner's inbox
// until Features/Auth grows a real "request access" API.
export const ACCESS_REQUEST_MAILTO =
  "mailto:tlokhande00@gmail.com?subject=Finora%20access%20request&body=Hi%2C%20I%27d%20like%20access%20to%20Finora.%20My%20Google%20email%20is%3A%20";
