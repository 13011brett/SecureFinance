import React from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { 
  Tabs, 
  Tab, 
  Box
} from '@mui/material';
import {
  TrendingUp,
  CurrencyBitcoin,
  Dashboard as DashboardIcon,
  AdminPanelSettings
} from '@mui/icons-material';

const Navigation: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();

  const handleChange = (event: React.SyntheticEvent, newValue: string) => {
    navigate(newValue);
  };

  return (
    <Box sx={{ borderBottom: 1, borderColor: 'divider', bgcolor: 'background.paper' }}>
      <Tabs 
        value={location.pathname} 
        onChange={handleChange} 
        centered
        sx={{
          '& .MuiTab-root': {
            minWidth: 160,
            fontWeight: 500,
          }
        }}
      >
        <Tab 
          icon={<DashboardIcon />} 
          label="Dashboard" 
          value="/" 
          iconPosition="start"
        />
        <Tab 
          icon={<TrendingUp />} 
          label="Stock Data" 
          value="/stocks" 
          iconPosition="start"
        />
        <Tab 
          icon={<CurrencyBitcoin />} 
          label="Cryptocurrency" 
          value="/crypto" 
          iconPosition="start"
        />
        <Tab 
          icon={<AdminPanelSettings />} 
          label="Admin Panel" 
          value="/admin" 
          iconPosition="start"
        />
      </Tabs>
    </Box>
  );
};

export default Navigation;