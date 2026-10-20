import React from 'react';
import { Outlet } from 'react-router-dom';
import RoleSwitcher from '../../components/common/RoleSwitcher';

/**
 * AuthLayout: Khung bọc cho các trang Login, Register, Forgot Password
 */
export default function AuthLayout() {
  return (
    <>
      <Outlet />
      <RoleSwitcher />
    </>
  );
}
