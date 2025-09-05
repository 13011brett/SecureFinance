import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  TextField,
  Button,
  Alert,
  Chip,
  CircularProgress,
  Stack
} from '@mui/material';
import { TrendingUp, TrendingDown } from '@mui/icons-material';
import { apiService, CryptoData as CryptoDataType } from '../services/apiService';

const CryptoData: React.FC = () => {
  const [symbol, setSymbol] = useState('bitcoin');
  const [cryptoData, setCryptoData] = useState<CryptoDataType | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>('');
  const [responseTime, setResponseTime] = useState<number>(0);

  const popularCryptos = ['bitcoin', 'ethereum', 'cardano', 'polkadot', 'chainlink'];

  const fetchCryptoData = async (cryptoSymbol?: string) => {
    const targetSymbol = cryptoSymbol || symbol;
    if (!targetSymbol) return;
    
    setLoading(true);
    setError('');
    
    try {
      const response = await apiService.getCrypto(targetSymbol.toLowerCase());
      if (response.data.success) {
        setCryptoData(response.data.data!);
        setResponseTime(response.data.responseTimeMs);
        setSymbol(targetSymbol);
      } else {
        setError(response.data.errorMessage || 'Failed to fetch crypto data');
      }
    } catch (err: any) {
      setError(err.response?.data?.errorMessage || 'Network error occurred');
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(value);
  };

  const formatMarketCap = (value: number) => {
    if (value >= 1e12) return `$${(value / 1e12).toFixed(2)}T`;
    if (value >= 1e9) return `$${(value / 1e9).toFixed(2)}B`;
    if (value >= 1e6) return `$${(value / 1e6).toFixed(2)}M`;
    return formatCurrency(value);
  };

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom sx={{ mb: 4, fontWeight: 'bold' }}>
        ₿ Cryptocurrency Data
      </Typography>

      {/* Input Section */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Stack spacing={2} direction={{ xs: 'column', md: 'row' }} alignItems="center">
            <TextField
              fullWidth
              label="Cryptocurrency ID"
              value={symbol}
              onChange={(e) => setSymbol(e.target.value.toLowerCase())}
              placeholder="e.g., bitcoin, ethereum, cardano"
              variant="outlined"
              sx={{ flex: 1 }}
            />
            <Button
              variant="contained"
              onClick={() => fetchCryptoData()}
              disabled={loading || !symbol}
              sx={{ height: 56, minWidth: 200 }}
            >
              {loading ? <CircularProgress size={24} /> : 'Get Crypto Data'}
            </Button>
          </Stack>
          
          <Box sx={{ mt: 2 }}>
            <Typography variant="subtitle2" sx={{ mb: 1 }}>Popular Cryptocurrencies:</Typography>
            <Box display="flex" gap={1} flexWrap="wrap">
              {popularCryptos.map((crypto) => (
                <Chip
                  key={crypto}
                  label={crypto}
                  onClick={() => fetchCryptoData(crypto)}
                  variant={symbol === crypto ? "filled" : "outlined"}
                  clickable
                />
              ))}
            </Box>
          </Box>
        </CardContent>
      </Card>

      {/* Error Alert */}
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {/* Crypto Data Display */}
      {cryptoData && (
        <Stack spacing={3} direction={{ xs: 'column', md: 'row' }}>
          <Card sx={{ flex: 2 }}>
            <CardContent>
              <Box display="flex" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
                <Typography variant="h5" fontWeight="bold">
                  {cryptoData.name} ({cryptoData.symbol})
                </Typography>
                <Chip 
                  label={`${responseTime}ms`} 
                  color="primary" 
                  size="small"
                />
              </Box>
              
              <Typography variant="h3" sx={{ mb: 1, fontWeight: 'bold' }}>
                {formatCurrency(cryptoData.price)}
              </Typography>
              
              <Box display="flex" alignItems="center" sx={{ mb: 2 }}>
                {cryptoData.changePercent24h >= 0 ? <TrendingUp color="success" /> : <TrendingDown color="error" />}
                <Typography 
                  variant="h6" 
                  color={cryptoData.changePercent24h >= 0 ? 'success.main' : 'error.main'}
                  sx={{ ml: 1 }}
                >
                  {cryptoData.changePercent24h >= 0 ? '+' : ''}{cryptoData.changePercent24h.toFixed(2)}% (24h)
                </Typography>
              </Box>
              
              <Typography variant="h6" sx={{ mb: 1 }}>
                Market Cap: {formatMarketCap(cryptoData.marketCap)}
              </Typography>
              
              <Typography variant="body2" color="textSecondary">
                Last Updated: {new Date(cryptoData.lastUpdated).toLocaleString()}
              </Typography>
            </CardContent>
          </Card>
          
          <Card sx={{ flex: 1 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                🔗 Data Source
              </Typography>
              <Alert severity="info" sx={{ mb: 2 }}>
                <Typography variant="subtitle2">CoinGecko API</Typography>
                <Typography variant="body2">Real-time cryptocurrency data with 60-second caching</Typography>
              </Alert>
              <Alert severity="success">
                <Typography variant="subtitle2">Performance</Typography>
                <Typography variant="body2">Response Time: {responseTime}ms</Typography>
                <Typography variant="body2">Rate Limit: 50 req/min</Typography>
              </Alert>
            </CardContent>
          </Card>
        </Stack>
      )}
    </Box>
  );
};

export default CryptoData;