import { suspendMember } from "../../api/access.api";
import { useOptimisticMemberMutation } from "./useOptimisticMemberMutation";

export const useSuspendMember = () => useOptimisticMemberMutation(suspendMember, "Suspended", "Couldn't suspend this member.");
