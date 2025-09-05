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
  Paper,
  Stack
} from '@mui/material';
import { TrendingUp, TrendingDown, Security } from '@mui/icons-material';
import { apiService, StockData as StockDataType } from '../services/apiService';

const StockData: React.FC = () => {
  const [symbol, setSymbol] = useState('AAPL');
  const [stockData, setStockData] = useState<StockDataType | null>(null);
  const [encryptedData, setEncryptedData] = useState<string>('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>('');
  const [responseTime, setResponseTime] = useState<number>(0);

  const fetchStockData = async () => {
    if (!symbol) return;
    
    setLoading(true);
    setError('');
    
    try {
      const response = await apiService.getStock(symbol.toUpperCase());
      if (response.data.success) {
        setStockData(response.data.data!);
        setResponseTime(response.data.responseTimeMs);
      } else {
        setError(response.data.errorMessage || 'Failed to fetch stock data');
      }
    } catch (err: any) {
      setError(err.response?.data?.errorMessage || 'Network error occurred');
    } finally {
      setLoading(false);
    }
  };

  const fetchEncryptedData = async () => {
    if (!symbol) return;
    
    setLoading(true);
    setError('');
    
    try {
      const response = await apiService.getStockEncrypted(symbol.toUpperCase());
      if (response.data.success) {
        setEncryptedData(response.data.data!);
        setResponseTime(response.data.responseTimeMs);
      } else {
        setError(response.data.errorMessage || 'Failed to fetch encrypted data');
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

  const formatPercent = (value: number) => {
    return `${value >= 0 ? '+' : ''}${value.toFixed(2)}%`;
  };

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom sx={{ mb: 4, fontWeight: 'bold' }}>
        📈 Real-Time Stock Data
      </Typography>

      {/* Input Section */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Stack spacing={2} direction={{ xs: 'column', md: 'row' }} alignItems="center">
            <TextField
              fullWidth
              label="Stock Symbol"
              value={symbol}
              onChange={(e) => setSymbol(e.target.value.toUpperCase())}
              placeholder="e.g., AAPL, GOOGL, MSFT"
              variant="outlined"
              sx={{ flex: 1 }}
            />
            <Button
              variant="contained"
              onClick={fetchStockData}
              disabled={loading || !symbol}
              sx={{ height: 56, minWidth: 150 }}
            >
              {loading ? <CircularProgress size={24} /> : 'Get Stock Data'}
            </Button>
            <Button
              variant="outlined"
              onClick={fetchEncryptedData}
              disabled={loading || !symbol}
              startIcon={<Security />}
              sx={{ height: 56, minWidth: 180 }}
            >
              Get Encrypted Data
            </Button>
          </Stack>
        </CardContent>
      </Card>

      {/* Error Alert */}
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {/* Stock Data Display */}
      {stockData && (
        <Stack spacing={3} direction={{ xs: 'column', md: 'row' }} sx={{ mb: 3 }}>
          <Card sx={{ flex: 1 }}>
            <CardContent>
              <Box display="flex" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
                <Typography variant="h5" fontWeight="bold">
                  {stockData.symbol}
                </Typography>
                <Chip 
                  label={`${responseTime}ms`} 
                  color="primary" 
                  size="small"
                />
              </Box>
              
              <Typography variant="h3" sx={{ mb: 1, fontWeight: 'bold' }}>
                {formatCurrency(stockData.price)}
              </Typography>
              
              <Box display="flex" alignItems="center" sx={{ mb: 2 }}>
                {stockData.change >= 0 ? <TrendingUp color="success" /> : <TrendingDown color="error" />}
                <Typography 
                  variant="h6" 
                  color={stockData.change >= 0 ? 'success.main' : 'error.main'}
                  sx={{ ml: 1 }}
                >
                  {formatCurrency(stockData.change)} ({formatPercent(stockData.changePercent)})
                </Typography>
              </Box>
              
              <Typography variant="body2" color="textSecondary">
                Volume: {stockData.volume.toLocaleString()}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Source: {stockData.source}
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Last Updated: {new Date(stockData.lastUpdated).toLocaleString()}
              </Typography>
            </CardContent>
          </Card>
          
          <Card sx={{ flex: 1 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                📊 Technical Details
              </Typography>
              <Alert severity="info" sx={{ mb: 2 }}>
                <Typography variant="subtitle2">Response Time: {responseTime}ms</Typography>
                <Typography variant="body2">Data served from Alpha Vantage API with Redis caching</Typography>
              </Alert>
              <Alert severity="success">
                <Typography variant="subtitle2">Security Features</Typography>
                <Typography variant="body2">• Rate limiting: 100 requests/minute</Typography>
                <Typography variant="body2">• Redis caching for performance</Typography>
                <Typography variant="body2">• Real-time data aggregation</Typography>
              </Alert>
            </CardContent>
          </Card>
        </Stack>
      )}

      {/* Encrypted Data Display */}
      {encryptedData && (
        <Card>
          <CardContent>
            <Box display="flex" alignItems="center" sx={{ mb: 2 }}>
              <Security color="primary" sx={{ mr: 1 }} />
              <Typography variant="h6">
                RSA/AES Encrypted Response
              </Typography>
            </Box>
            <Alert severity="warning" sx={{ mb: 2 }}>
              <Typography variant="subtitle2">🔒 Enterprise Security Showcase</Typography>
              <Typography variant="body2">
                This demonstrates our hybrid encryption methodology using RSA for key exchange and AES for data encryption.
              </Typography>
            </Alert>
            <Paper sx={{ p: 2, bgcolor: 'grey.900', fontFamily: 'monospace', overflow: 'auto' }}>
              <Typography variant="body2" sx={{ wordBreak: 'break-all', fontSize: '0.75rem' }}>
                {encryptedData}
              </Typography>
            </Paper>
          </CardContent>
        </Card>
      )}
    </Box>
  );
};

export default StockData;