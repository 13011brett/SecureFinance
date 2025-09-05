// SecureFinance.Web/src/components/Dashboard.tsx - SIMPLIFIED VERSION
import React, { useState, useEffect } from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box,
  Chip,
  CircularProgress,
  Alert,
  Paper,
  Stack
} from '@mui/material';
import {
  TrendingUp,
  Security,
  Speed,
  CloudDone
} from '@mui/icons-material';

interface SystemHealth {
  status: string;
  database: string;
  timestamp: string;
  dataSources: Array<{
    name: string;
    isActive: boolean;
    lastHealthCheck: string;
  }>;
}

interface UsageStats {
  totalRequests: number;
  cachedRequests: number;
  averageResponseTime: number;
  errorCount: number;
  requestsByEndpoint: Record<string, number>;
}

const Dashboard: React.FC = () => {
  const [health, setHealth] = useState<SystemHealth | null>(null);
  const [stats, setStats] = useState<UsageStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Mock data for demo (replace with API calls later)
    setTimeout(() => {
      setHealth({
        status: 'Healthy',
        database: 'Connected',
        timestamp: new Date().toISOString(),
        dataSources: [
          { name: 'Alpha Vantage', isActive: true, lastHealthCheck: new Date().toISOString() },
          { name: 'CoinGecko', isActive: true, lastHealthCheck: new Date().toISOString() },
          { name: 'FRED', isActive: true, lastHealthCheck: new Date().toISOString() }
        ]
      });
      
      setStats({
        totalRequests: 1547,
        cachedRequests: 1241,
        averageResponseTime: 156,
        errorCount: 3,
        requestsByEndpoint: {
          '/api/stock/*': 847,
          '/api/crypto/*': 423,
          '/api/rates/*': 277
        }
      });
      
      setLoading(false);
    }, 1000);
  }, []);

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress size={60} />
      </Box>
    );
  }

  const cacheHitRate = stats ? ((stats.cachedRequests / stats.totalRequests) * 100).toFixed(1) : '0';

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom sx={{ mb: 4, fontWeight: 'bold' }}>
        SecureFinance API Dashboard
      </Typography>

      {/* System Status Cards */}
      <Stack spacing={3} direction={{ xs: 'column', md: 'row' }} sx={{ mb: 4 }}>
        <Card sx={{ flex: 1, background: 'linear-gradient(135deg, #1e3c72 0%, #2a5298 100%)' }}>
          <CardContent>
            <Box display="flex" alignItems="center" justifyContent="space-between">
              <Box>
                <Typography color="textSecondary" gutterBottom>
                  System Status
                </Typography>
                <Chip 
                  label={health?.status || 'Unknown'} 
                  color={health?.status === 'Healthy' ? 'success' : 'error'}
                  icon={<CloudDone />}
                />
              </Box>
              <Security sx={{ fontSize: 40, opacity: 0.7 }} />
            </Box>
          </CardContent>
        </Card>

        <Card sx={{ flex: 1, background: 'linear-gradient(135deg, #134e5e 0%, #71b280 100%)' }}>
          <CardContent>
            <Box display="flex" alignItems="center" justifyContent="space-between">
              <Box>
                <Typography color="textSecondary" gutterBottom>
                  Total Requests
                </Typography>
                <Typography variant="h5" component="div">
                  {stats?.totalRequests?.toLocaleString() || '0'}
                </Typography>
              </Box>
              <TrendingUp sx={{ fontSize: 40, opacity: 0.7 }} />
            </Box>
          </CardContent>
        </Card>

        <Card sx={{ flex: 1, background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' }}>
          <CardContent>
            <Box display="flex" alignItems="center" justifyContent="space-between">
              <Box>
                <Typography color="textSecondary" gutterBottom>
                  Cache Hit Rate
                </Typography>
                <Typography variant="h5" component="div">
                  {cacheHitRate}%
                </Typography>
              </Box>
              <Speed sx={{ fontSize: 40, opacity: 0.7 }} />
            </Box>
          </CardContent>
        </Card>

        <Card sx={{ flex: 1, background: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)' }}>
          <CardContent>
            <Box display="flex" alignItems="center" justifyContent="space-between">
              <Box>
                <Typography color="textSecondary" gutterBottom>
                  Avg Response Time
                </Typography>
                <Typography variant="h5" component="div">
                  {stats?.averageResponseTime?.toFixed(0) || '0'}ms
                </Typography>
              </Box>
            </Box>
          </CardContent>
        </Card>
      </Stack>

      {/* Data Sources and Usage */}
      <Stack spacing={3} direction={{ xs: 'column', md: 'row' }} sx={{ mb: 4 }}>
        <Card sx={{ flex: 1 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Data Sources Status
            </Typography>
            {health?.dataSources?.map((source, index) => (
              <Box key={index} display="flex" justifyContent="space-between" alignItems="center" sx={{ mb: 1 }}>
                <Typography>{source.name}</Typography>
                <Chip 
                  label={source.isActive ? 'Active' : 'Inactive'} 
                  color={source.isActive ? 'success' : 'error'}
                  size="small"
                />
              </Box>
            ))}
          </CardContent>
        </Card>

        <Card sx={{ flex: 1 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              API Endpoint Usage
            </Typography>
            {stats?.requestsByEndpoint && Object.entries(stats.requestsByEndpoint).map(([endpoint, count]) => (
              <Box key={endpoint} display="flex" justifyContent="space-between" sx={{ mb: 1 }}>
                <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                  {endpoint}
                </Typography>
                <Typography variant="body2" fontWeight="bold">
                  {count}
                </Typography>
              </Box>
            ))}
          </CardContent>
        </Card>
      </Stack>

      {/* Key Features Section */}
      <Paper sx={{ p: 3, background: 'linear-gradient(135deg, #1a1a2e 0%, #16213e 100%)' }}>
        <Typography variant="h5" gutterBottom sx={{ mb: 3 }}>
          🚀 Enterprise Features
        </Typography>
        <Stack spacing={2} direction={{ xs: 'column', md: 'row' }}>
          <Alert severity="success" sx={{ flex: 1 }}>
            <Typography variant="subtitle2" fontWeight="bold">RSA/AES Hybrid Encryption</Typography>
            <Typography variant="body2">Military-grade security for all sensitive data</Typography>
          </Alert>
          <Alert severity="info" sx={{ flex: 1 }}>
            <Typography variant="subtitle2" fontWeight="bold">Intelligent Caching</Typography>
            <Typography variant="body2">Redis-based caching with {cacheHitRate}% hit rate</Typography>
          </Alert>
          <Alert severity="warning" sx={{ flex: 1 }}>
            <Typography variant="subtitle2" fontWeight="bold">Rate Limiting</Typography>
            <Typography variant="body2">100 requests/minute with graceful degradation</Typography>
          </Alert>
        </Stack>
      </Paper>
    </Box>
  );
};

export default Dashboard;