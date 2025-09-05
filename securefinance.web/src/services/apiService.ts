import axios, { AxiosResponse } from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:8080';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
    'X-API-Key': 'sf_demo_key_for_portfolio_showcase'
  }
});

// Request interceptor to add timestamp
apiClient.interceptors.request.use(
  (config) => {
    console.log(`[API] ${config.method?.toUpperCase()} ${config.url}`);
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor for logging
apiClient.interceptors.response.use(
  (response) => {
    console.log(`[API] Response: ${response.status} in ${response.data?.responseTimeMs}ms`);
    return response;
  },
  (error) => {
    console.error('[API] Error:', error.response?.data || error.message);
    return Promise.reject(error);
  }
);

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  errorMessage?: string;
  timestamp: string;
  fromCache: boolean;
  responseTimeMs: number;
}

export interface StockData {
  symbol: string;
  price: number;
  change: number;
  changePercent: number;
  volume: number;
  lastUpdated: string;
  source: string;
}

export interface CryptoData {
  symbol: string;
  name: string;
  price: number;
  change24h: number;
  changePercent24h: number;
  marketCap: number;
  lastUpdated: string;
}

export interface ExchangeRate {
  fromCurrency: string;
  toCurrency: string;
  rate: number;
  lastUpdated: string;
}

export const apiService = {
  // Stock data endpoints
  getStock: (symbol: string): Promise<AxiosResponse<ApiResponse<StockData>>> =>
    apiClient.get(`/api/stock/${symbol}`),

  getStockEncrypted: (symbol: string): Promise<AxiosResponse<ApiResponse<string>>> =>
    apiClient.get(`/api/stock/${symbol}/encrypted`),

  // Crypto data endpoints
  getCrypto: (symbol: string): Promise<AxiosResponse<ApiResponse<CryptoData>>> =>
    apiClient.get(`/api/crypto/${symbol}`),

  // Exchange rate endpoints
  getExchangeRate: (from: string, to: string): Promise<AxiosResponse<ApiResponse<ExchangeRate>>> =>
    apiClient.get(`/api/rates/currency/${from}/${to}`),

  // Admin endpoints
  getUsageStats: (): Promise<AxiosResponse<ApiResponse<any>>> =>
    apiClient.get('/api/admin/usage'),

  getSystemHealth: (): Promise<AxiosResponse<ApiResponse<any>>> =>
    apiClient.get('/api/admin/health'),
};